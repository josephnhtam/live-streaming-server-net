using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Logging;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal
{
    internal class AzureSpeechTranscriptionStream : ITranscriptionStream
    {
        private readonly IConversationTranscriberFactory _transcriberFactory;
        private readonly ITranscodingStreamFactory _transcodingStreamFactory;
        private readonly ILogger _logger;

        private readonly int _id;
        private readonly CancellationTokenSource _cts;
        private readonly Channel<AudioBuffer> _audioBufferChannel;

        private int _isStarted;
        private int _isStopped;
        private int _isDisposed;
        private Task? _transcriptionTask;

        private uint? _initialTimestamp;
        private uint _lastTimestamp;
        private uint _timestampOffset;

        public event EventHandler<TranscriptionStartedEventArgs>? TranscriptionStarted;
        public event EventHandler<TranscriptionStoppedEventArgs>? TranscriptionStopped;
        public event EventHandler<TranscriptionCanceledEventArgs>? TranscriptionCanceled;
        public event EventHandler<TranscriptionResultReceivedEventArgs>? TranscriptionResultReceived;

        private static int _nextId;

        public AzureSpeechTranscriptionStream(
            IConversationTranscriberFactory transcriberFactory,
            ITranscodingStreamFactory transcodingStreamFactory,
            ILogger<AzureSpeechTranscriptionStream> logger)
        {
            _transcriberFactory = transcriberFactory;
            _transcodingStreamFactory = transcodingStreamFactory;
            _logger = logger;

            _id = Interlocked.Increment(ref _nextId);
            _cts = new CancellationTokenSource();
            _audioBufferChannel = Channel.CreateUnbounded<AudioBuffer>(new() { AllowSynchronousContinuations = true });
        }

        public ValueTask StartAsync()
        {
            if (Interlocked.Exchange(ref _isStarted, 1) == 1)
            {
                return ValueTask.CompletedTask;
            }

            _logger.TranscriptionStreamStarted(_id);
            TranscriptionStarted?.Invoke(this, new TranscriptionStartedEventArgs(_id));
            _transcriptionTask = RunTranscriptionAsync();
            return ValueTask.CompletedTask;
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _isStopped, 1) == 1)
            {
                return;
            }

            try
            {
                _cts.Cancel();
                TranscriptionCanceled?.Invoke(this, new TranscriptionCanceledEventArgs(null));

                if (_transcriptionTask != null)
                {
                    await _transcriptionTask.WithCancellation(cancellationToken);
                }
            }
            finally
            {
                TranscriptionStopped?.Invoke(this, new TranscriptionStoppedEventArgs(_id));
                _logger.TranscriptionStreamStopped(_id);
            }
        }

        private async Task RunTranscriptionAsync()
        {
            var cancellationToken = _cts.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await using var transcodingStream = _transcodingStreamFactory.Create();
                    await DoRunTranscriptionAsync(transcodingStream, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.ErrorRunningTranscription(ex);
                }
            }
        }

        private async Task DoRunTranscriptionAsync(ITranscodingStream transcodingStream, CancellationToken stoppingToken)
        {
            AdjustTimestampOffset();

            var transcodingTcs = new TaskCompletionSource();
            var transcriptingTcs = new TaskCompletionSource();

            using var transcriptionCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var transcriptionCancellation = transcriptionCts.Token;

            using var pushStream = AudioInputStream.CreatePushStream();
            using var audioInput = AudioConfig.FromStreamInput(pushStream);

            using var transcriber = _transcriberFactory.Create(audioInput);

            ConfigureEvents(transcodingStream, transcriber, pushStream, transcriptionCts, transcodingTcs, transcriptingTcs);

            await transcriber.StartTranscribingAsync();
            await transcodingStream.StartAsync();

            _logger.SpeechRecognitionStarted();

            try
            {
                await StreamAudioToTranscoderAsync(transcodingStream, transcriptionCancellation);
            }
            catch (OperationCanceledException) when (transcriptionCancellation.IsCancellationRequested)
            {
                _logger.TranscriptionCanceledLog();
            }
            catch (Exception ex)
            {
                _logger.ErrorRunningTranscription(ex);
                throw;
            }
            finally
            {
                await ErrorBoundary.ExecuteAsync(async () => await transcodingStream.StopAsync(stoppingToken));
                await ErrorBoundary.ExecuteAsync(transcriber.StartTranscribingAsync);
                _logger.SpeechRecognitionStopped();
            }

            await Task.WhenAll(transcodingTcs.Task, transcriptingTcs.Task);
        }

        private void AdjustTimestampOffset()
        {
            if (!_initialTimestamp.HasValue)
                return;

            _timestampOffset = _lastTimestamp - _initialTimestamp.Value;
        }

        private async Task StreamAudioToTranscoderAsync(ITranscodingStream transcodingStream, CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var audioBuffer in _audioBufferChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        _initialTimestamp ??= audioBuffer.Timestamp;
                        _lastTimestamp = audioBuffer.Timestamp;

                        await transcodingStream.WriteAsync(MediaType.Audio, audioBuffer.RentedBuffer, audioBuffer.Timestamp, cancellationToken);
                    }
                    finally
                    {
                        audioBuffer.RentedBuffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.ErrorStreamingAudioToTranscoder(ex);
                throw;
            }
            finally
            {
                _logger.AudioStreamingStopped();
            }
        }

        private void ConfigureEvents(
            ITranscodingStream transcodingStream,
            ConversationTranscriber transcriber,
            PushAudioInputStream pushStream,
            CancellationTokenSource transcriptionCts,
            TaskCompletionSource transcodingTcs,
            TaskCompletionSource transcriptingTcs)
        {
            transcodingStream.TranscodingCanceled += (s, e) =>
            {
                _logger.TranscodingCanceledLog();

                if (e.Exception != null)
                {
                    transcodingTcs.TrySetException(e.Exception);
                }

                transcriptionCts.Cancel();
            };

            transcodingStream.TranscodingStopped += (s, e) =>
            {
                _logger.TranscodingStoppedLog();
                transcriptionCts.Cancel();
                transcodingTcs.TrySetResult();
            };

            transcodingStream.TranscodedBufferReceived.Register((s, e) =>
            {
                pushStream.Write(e.RentedBuffer.Buffer, e.RentedBuffer.Size);
                return Task.CompletedTask;
            });

            transcriber.SessionStarted += (s, e) =>
            {
                _logger.RecognizerSessionStarted(e.SessionId);
            };

            transcriber.SessionStopped += (s, e) =>
            {
                _logger.RecognizerSessionStopped(e.SessionId);
                transcriptionCts.Cancel();
                transcriptingTcs.TrySetResult();
            };

            transcriber.Canceled += (s, e) =>
            {
                _logger.RecognizerCanceled(e.ErrorCode.ToString(), e.ErrorDetails);

                if (e.Reason == CancellationReason.Error)
                {
                    transcriptingTcs.TrySetException(new Exception(e.ErrorDetails));
                }

                transcriptionCts.Cancel();
            };

            transcriber.Transcribing += (s, e) =>
                ErrorBoundary.Execute(() =>
                {
                    var baseTimestamp = _initialTimestamp.HasValue ?
                        TimeSpan.FromMilliseconds(_initialTimestamp.Value + _timestampOffset) :
                        TimeSpan.Zero;

                    var text = e.Result.Text;
                    var timestamp = baseTimestamp + TimeSpan.FromTicks(e.Result.OffsetInTicks);
                    var duration = TimeSpan.FromTicks(e.Result.Duration.Ticks);

                    TranscriptionResultReceived?.Invoke(this, new TranscriptionResultReceivedEventArgs(
                        new TranscriptionResult(text, timestamp, duration)));

                    _logger.RecognizingText(text, timestamp, duration);
                });
        }

        public async ValueTask SendAsync(IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken)
        {
            rentedBuffer.Claim();

            try
            {
                await _audioBufferChannel.Writer.WriteAsync(new AudioBuffer(rentedBuffer, timestamp), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                rentedBuffer.Unclaim();
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
                throw;
            }
        }

        private void ReleaseBuffers()
        {
            while (_audioBufferChannel.Reader.TryRead(out var audioBuffer))
            {
                audioBuffer.RentedBuffer.Unclaim();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            {
                return;
            }

            await ErrorBoundary.ExecuteAsync(async () => await StopAsync(default));

            _audioBufferChannel.Writer.Complete();

            if (_transcriptionTask != null)
            {
                await _transcriptionTask;
            }

            ReleaseBuffers();

            _cts.Dispose();
        }

        public Exception? GetException()
        {
            if (_transcriptionTask != null)
            {
                return _transcriptionTask.Exception;
            }

            return null;
        }

        private record struct AudioBuffer(IRentedBuffer RentedBuffer, uint Timestamp);
    }
}