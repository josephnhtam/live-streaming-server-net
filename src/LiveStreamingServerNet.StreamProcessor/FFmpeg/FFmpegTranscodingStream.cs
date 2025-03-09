using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Configurations;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Diagnostics;
using System.Threading.Channels;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg
{
    public class FFmpegTranscodingStream : ITranscodingStream
    {
        private readonly Process _process;
        private readonly IMediaStreamWriter _inputStreamWriter;
        private readonly IDataBufferPool _dataBufferPool;

        private readonly Channel<MediaBuffer> _sendBufferChannel;
        private readonly CancellationTokenSource _cts;

        private int _isStarted;
        private int _isStopped;
        private int _isHeaderWritten;
        private int _isDisposed;
        private Task? _transcodingTask;

        public event EventHandler<TranscodingStartedEventArgs>? TranscodingStarted;
        public event EventHandler<TranscodingStoppedEventArgs>? TranscodingStopped;
        public event EventHandler<TranscodingCanceledEventArgs>? TranscodingCanceled;

        private readonly AsyncEventHandler<TranscodedBufferReceivedEventArgs> _transcodedBufferReceived = new();
        public IAsyncEventHandler<TranscodedBufferReceivedEventArgs> TranscodedBufferReceived => _transcodedBufferReceived;

        public FFmpegTranscodingStream(
            IDataBufferPool dataBufferPool,
            IMediaStreamWriterFactory inputStreamWriterFactory,
            FFmpegTranscodingStreamConfiguration config)
        {
            _process = CreateFFmpegProcess(config);
            _inputStreamWriter = inputStreamWriterFactory.Create(new StandardInputStreamWriter(_process));
            _dataBufferPool = dataBufferPool;

            _sendBufferChannel = Channel.CreateUnbounded<MediaBuffer>(new() { AllowSynchronousContinuations = true });
            _cts = new CancellationTokenSource();
        }

        private Process CreateFFmpegProcess(FFmpegTranscodingStreamConfiguration config)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = config.FFmpegPath,
                    Arguments = config.FFmpegArguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            return process;
        }

        public ValueTask StartAsync()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
            {
                return ValueTask.CompletedTask;
            }

            _process.Start();
            TranscodingStarted?.Invoke(this, new TranscodingStartedEventArgs(_process.Id));

            _transcodingTask = ProcessTranscodingAsync(_cts.Token);
            return ValueTask.CompletedTask;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isStopped, 1, 0) == 1)
            {
                return ValueTask.CompletedTask;
            }

            try
            {
                _cts.Cancel();

                if (!_process.HasExited)
                    _process.Kill();
            }
            finally
            {
                TranscodingStopped?.Invoke(this, new(_process.Id));
            }

            return ValueTask.CompletedTask;
        }

        private async Task ProcessTranscodingAsync(CancellationToken cancellationToken)
        {
            try
            {
                var groupCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                await Task.WhenAll(
                    WriteBufferAsync(groupCts),
                    ReceiveTranscodedBufferAsync(groupCts)
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                TranscodingCanceled?.Invoke(this, new TranscodingCanceledEventArgs(null));
            }
            catch (Exception ex)
            {
                TranscodingCanceled?.Invoke(this, new TranscodingCanceledEventArgs(ex));
            }
        }

        private async Task ReceiveTranscodedBufferAsync(CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;

            try
            {
                int bytesRead;
                var buffer = new byte[4096];

                while ((bytesRead = await _process.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    var rentedBuffer = new RentedBuffer(_dataBufferPool.BufferPool, bytesRead);
                    buffer.AsSpan(0, bytesRead).CopyTo(rentedBuffer.Buffer);

                    try
                    {
                        await _transcodedBufferReceived.InvokeAsync(this, new TranscodedBufferReceivedEventArgs(rentedBuffer));
                    }
                    finally
                    {
                        rentedBuffer.Unclaim();
                    }
                }

                if (_process.HasExited && _process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg process exited with code {_process.ExitCode}");
                }
            }
            finally
            {
                cts.Cancel();
            }
        }

        private async Task WriteBufferAsync(CancellationTokenSource cts)
        {
            var cancellationToken = cts.Token;

            try
            {
                await foreach (var mediaBuffer in _sendBufferChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await SendHeaderIfNeededAsync(cancellationToken);

                        await _inputStreamWriter.WriteBufferAsync(
                            mediaBuffer.MediaType, mediaBuffer.RentedBuffer, mediaBuffer.Timestamp, cancellationToken);
                    }
                    finally
                    {
                        mediaBuffer.RentedBuffer.Unclaim();
                    }
                }
            }
            finally
            {
                cts.Cancel();
            }
        }

        public async ValueTask WriteAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken)
        {
            rentedBuffer.Claim();

            try
            {
                await _sendBufferChannel.Writer.WriteAsync(new MediaBuffer(mediaType, rentedBuffer, timestamp), cancellationToken);
            }
            catch (Exception)
            {
                rentedBuffer.Unclaim();
                throw;
            }
        }

        private async Task SendHeaderIfNeededAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isHeaderWritten, 1, 0) == 0)
            {
                await _inputStreamWriter.WriteHeaderAsync(cancellationToken);
            }
        }

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }

        ~FFmpegTranscodingStream()
        {
            DisposeAsync(false).AsTask().Wait();
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            {
                return;
            }

            await ErrorBoundary.ExecuteAsync(async () => await StopAsync(default));

            _sendBufferChannel.Writer.Complete();

            if (disposing)
            {
                if (_transcodingTask != null)
                {
                    await ErrorBoundary.ExecuteAsync(_transcodingTask);
                }

                await _process.StandardInput.DisposeAsync();
                _process.StandardOutput.Dispose();
                _process.Dispose();
            }

            ReleaseBuffers();

            await _inputStreamWriter.DisposeAsync();
        }

        private void ReleaseBuffers()
        {
            while (_sendBufferChannel.Reader.TryRead(out var mediaBuffer))
            {
                mediaBuffer.RentedBuffer.Unclaim();
            }
        }

        private class StandardInputStreamWriter : IStreamWriter
        {
            private readonly Process _process;

            public StandardInputStreamWriter(Process process)
            {
                _process = process;
            }

            public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
            {
                return _process.StandardInput.BaseStream.WriteAsync(buffer, cancellationToken);
            }
        }

        private record struct MediaBuffer(MediaType MediaType, IRentedBuffer RentedBuffer, uint Timestamp);
    }
}
