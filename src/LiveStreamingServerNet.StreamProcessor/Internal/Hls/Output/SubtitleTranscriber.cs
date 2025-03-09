using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal class SubtitleTranscriber : ISubtitleTranscriber
    {
        public SubtitleTrackOptions Options { get; }

        private readonly ITranscriptionStream _stream;
        private readonly CancellationTokenSource _cts;
        private readonly Channel<AudioBuffer> _audioBufferChannel;
        private readonly Channel<TranscriptionResult> _transcriptionResults;

        private Task? _publishingTask;
        private Task? _consumingTask;

        public SubtitleTranscriber(SubtitleTrackOptions options, ITranscriptionStream stream)
        {
            Options = options;
            _stream = stream;

            _cts = new CancellationTokenSource();
            _audioBufferChannel = Channel.CreateUnbounded<AudioBuffer>(
                new UnboundedChannelOptions { AllowSynchronousContinuations = true });
            _transcriptionResults = Channel.CreateUnbounded<TranscriptionResult>(
                new UnboundedChannelOptions { AllowSynchronousContinuations = true });
        }

        public async ValueTask StartAsync()
        {
            await _stream.StartAsync();
            _publishingTask = PublishAsync(_cts.Token);
        }

        public async ValueTask EnqueueAudioBufferAsync(IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();
            await _audioBufferChannel.Writer.WriteAsync(new AudioBuffer(rentedBuffer, timestamp));
        }

        public ValueTask<TranscriptionResult> ReceiveTranscriptionResultAsync(CancellationToken cancellationToken)
        {
            return _transcriptionResults.Reader.ReadAsync(cancellationToken);
        }

        private async Task PublishAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var audioBuffer in _audioBufferChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await _stream.SendAsync(audioBuffer.Buffer, audioBuffer.Timestamp, cancellationToken);
                    }
                    finally
                    {
                        audioBuffer.Buffer.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                // todo: log error
            }
            finally
            {
                _audioBufferChannel.Writer.Complete();

                while (_audioBufferChannel.Reader.TryRead(out var audioBuffer))
                {
                    audioBuffer.Buffer.Unclaim();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            await _stream.DisposeAsync();

            if (_publishingTask != null)
            {
                await _publishingTask;
            }

            if (_consumingTask != null)
            {
                await _consumingTask;
            }
        }

        private record struct AudioBuffer(IRentedBuffer Buffer, uint Timestamp);
    }
}
