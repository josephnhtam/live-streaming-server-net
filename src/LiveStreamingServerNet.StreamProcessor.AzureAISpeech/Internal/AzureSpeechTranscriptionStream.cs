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
        public event EventHandler<TranscribingResultReceivedEventArgs>? TranscribingResultReceived;
        public event EventHandler<TranscribedResultReceivedEventArgs>? TranscribedResultReceived;

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
                    await _transcriptionTask.WithCancellation(cancellationToken).ConfigureAwait(false);
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
                    var transcodingStream = _transcodingStreamFactory.Create();
                    await using (transcodingStream.ConfigureAwait(false))
                    {
                        await DoRunTranscriptionAsync(transcodingStream, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorRunningTranscription(_id, ex);
                }
            }
        }

        private async Task DoRunTranscriptionAsync(ITranscodingStream transcodingStream, CancellationToken stoppingToken)
        {
            AdjustTimestampOffset();

            var transcodingTcs = new TaskCompletionSource();
            var transcribingTcs = new TaskCompletionSource();

            using var transcriptionCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var transcriptionCancellation = transcriptionCts.Token;

            using var pushStream = AudioInputStream.CreatePushStream();
            using var audioInput = AudioConfig.FromStreamInput(pushStream);

            using var transcriber = _transcriberFactory.Create(audioInput);

            ConfigureTranscodingEvents(transcodingStream, pushStream, transcriptionCts, transcodingTcs);
            ConfigureTranscribingEvents(transcriber, transcriptionCts, transcribingTcs);

            await transcriber.StartTranscribingAsync().ConfigureAwait(false);
            await transcodingStream.StartAsync().ConfigureAwait(false);

            _logger.TranscriptionStarted(_id);

            try
            {
                await StreamAudioToTranscoderAsync(transcodingStream, transcriptionCancellation).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (transcriptionCancellation.IsCancellationRequested)
            {
                _logger.TranscriptionCanceledLog(_id);
            }
            catch (Exception ex)
            {
                _logger.ErrorRunningTranscription(_id, ex);
                throw;
            }
            finally
            {
                await StopTranscriptionStream(transcribingTcs, transcriber).ConfigureAwait(false);
                await StopTranscodingStream(transcodingStream, transcodingTcs, stoppingToken).ConfigureAwait(false);

                _logger.TranscriptionStopped(_id);
            }

            await Task.WhenAll(transcodingTcs.Task, transcribingTcs.Task).ConfigureAwait(false);
        }

        private static async Task StopTranscriptionStream(TaskCompletionSource transcribingTcs, ConversationTranscriber transcriber)
        {
            try
            {
                await transcriber.StopTranscribingAsync().ConfigureAwait(false);
                transcribingTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                transcribingTcs.SetException(ex);
            }
        }

        private static async Task StopTranscodingStream(
            ITranscodingStream transcodingStream, TaskCompletionSource transcodingTcs, CancellationToken stoppingToken)
        {
            try
            {
                await transcodingStream.StopAsync(stoppingToken).ConfigureAwait(false);
                transcodingTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                transcodingTcs.SetException(ex);
            }
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
                await foreach (var audioBuffer in _audioBufferChannel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        _initialTimestamp ??= audioBuffer.Timestamp;
                        _lastTimestamp = audioBuffer.Timestamp;

                        await transcodingStream.WriteAsync(MediaType.Audio, audioBuffer.RentedBuffer, audioBuffer.Timestamp, cancellationToken).ConfigureAwait(false);
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
                _logger.ErrorStreamingAudioToTranscoder(_id, ex);
                throw;
            }
            finally
            {
                _logger.AudioStreamingStopped(_id);
            }
        }

        private void ConfigureTranscodingEvents(
            ITranscodingStream transcodingStream,
            PushAudioInputStream pushStream,
            CancellationTokenSource transcriptionCts,
            TaskCompletionSource transcodingTcs)
        {
            transcodingStream.TranscodingCanceled += (s, e) =>
            {
                _logger.TranscodingCanceledLog(_id);

                if (e.Exception != null)
                {
                    transcodingTcs.TrySetException(e.Exception);
                }

                transcriptionCts.Cancel();
            };

            transcodingStream.TranscodingStopped += (s, e) =>
            {
                _logger.TranscodingStoppedLog(_id);
                transcriptionCts.Cancel();
            };

            transcodingStream.TranscodedBufferReceived.Register((s, e) =>
            {
                pushStream.Write(e.RentedBuffer.Buffer, e.RentedBuffer.Size);
                return Task.CompletedTask;
            });
        }

        private void ConfigureTranscribingEvents(
            ConversationTranscriber transcriber,
            CancellationTokenSource transcriptionCts,
            TaskCompletionSource transcribingTcs)
        {
            transcriber.SessionStarted += (s, e) => { _logger.TranscriberSessionStarted(_id, e.SessionId); };

            transcriber.SessionStopped += (s, e) =>
            {
                _logger.TranscriberSessionStopped(_id, e.SessionId);
                transcriptionCts.Cancel();
            };

            transcriber.Canceled += (s, e) =>
            {
                _logger.TranscriberCanceled(_id, e.ErrorCode.ToString(), e.ErrorDetails);

                if (e.Reason == CancellationReason.Error)
                {
                    transcribingTcs.TrySetException(new Exception(e.ErrorDetails));
                }

                transcriptionCts.Cancel();
            };

            transcriber.Transcribing += (s, e) =>
                ErrorBoundary.Execute(() =>
                {
                    if (TranscribingResultReceived == null)
                    {
                        return;
                    }

                    var baseTimestamp = GetBaseTimestamp();

                    var resultId = e.Result.ResultId;
                    var speakerId = e.Result.SpeakerId;
                    var text = e.Result.Text;
                    var timestamp = baseTimestamp + TimeSpan.FromTicks(e.Result.OffsetInTicks);
                    var duration = e.Result.Duration;

                    TranscribingResultReceived.Invoke(this, new TranscribingResultReceivedEventArgs(
                        new TranscriptionResult(resultId, speakerId, text, timestamp, duration, null)));

                    _logger.TranscribingText(_id, resultId, speakerId, text, timestamp, duration);
                });

            transcriber.Transcribed += (s, e) =>
                ErrorBoundary.Execute(() =>
                {
                    if (TranscribedResultReceived == null)
                    {
                        return;
                    }

                    var baseTimestamp = GetBaseTimestamp();

                    var resultId = e.Result.ResultId;
                    var speakerId = e.Result.SpeakerId;
                    var text = e.Result.Text;
                    var timestamp = baseTimestamp + TimeSpan.FromTicks(e.Result.OffsetInTicks);
                    var duration = e.Result.Duration;

                    var words = e.Result.Best()?.FirstOrDefault()?.Words.Select(x =>
                        new TranscribedWord(x.Word, baseTimestamp + TimeSpan.FromTicks(x.Offset), TimeSpan.FromTicks(x.Duration))
                    ).ToList();

                    TranscribedResultReceived.Invoke(this, new TranscribedResultReceivedEventArgs(
                        new TranscriptionResult(resultId, speakerId, text, timestamp, duration, words)));

                    _logger.TranscribedText(_id, resultId, speakerId, text, timestamp, duration);
                });
        }

        private TimeSpan GetBaseTimestamp()
        {
            return _initialTimestamp.HasValue ? TimeSpan.FromMilliseconds(_initialTimestamp.Value + _timestampOffset) : TimeSpan.Zero;
        }

        public async ValueTask WriteAsync(IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken)
        {
            rentedBuffer.Claim();

            try
            {
                await _audioBufferChannel.Writer.WriteAsync(new AudioBuffer(rentedBuffer, timestamp), cancellationToken).ConfigureAwait(false);
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

            await ErrorBoundary.ExecuteAsync(async () => await StopAsync(default).ConfigureAwait(false)).ConfigureAwait(false);

            _audioBufferChannel.Writer.Complete();

            if (_transcriptionTask != null)
            {
                await _transcriptionTask.ConfigureAwait(false);
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