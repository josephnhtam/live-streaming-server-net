using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpChunkMessageSenderService : IRtmpChunkMessageSenderService
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly IRtmpChunkMessageWriterService _writer;

        public RtmpChunkMessageSenderService(INetBufferPool netBufferPool, IRtmpChunkMessageWriterService writer)
        {
            _netBufferPool = netBufferPool;
            _writer = writer;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action<bool>? callback) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            try
            {
                clientContext.Client.Send(targetBuffer =>
                    _writer.Write(targetBuffer, basicHeader, messageHeader, payloadBuffer, clientContext.OutChunkSize),
                    callback
                );
            }
            finally
            {
                _netBufferPool.Recycle(payloadBuffer);
            }
        }

        public ValueTask SendAsync<TRtmpChunkMessageHeader>(IRtmpClientContext clientContext, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new ValueTaskCompletionSource();
            Send(clientContext, basicHeader, messageHeader, payloadWriter, _ => tcs.SetResult());
            return tcs.Task;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IReadOnlyList<IRtmpClientContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            if (!clientContexts.Any())
                return;

            if (clientContexts.Count == 1)
            {
                Send(clientContexts[0], basicHeader, messageHeader, payloadWriter, null);
                return;
            }

            var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            try
            {
                foreach (var clientsGroup in clientContexts.GroupBy(x => x.OutChunkSize))
                {
                    var outChunkSize = clientsGroup.Key;
                    var clients = clientsGroup.Select(x => x.Client).ToList();

                    var targetBuffer = _netBufferPool.Obtain();

                    try
                    {
                        _writer.Write(targetBuffer, basicHeader, messageHeader, payloadBuffer.MoveTo(0), outChunkSize);

                        foreach (var client in clients)
                        {
                            client.Send(targetBuffer);
                        }
                    }
                    finally
                    {
                        _netBufferPool.Recycle(targetBuffer);
                    }
                }
            }
            finally
            {
                _netBufferPool.Recycle(payloadBuffer);
            }
        }

        private INetBuffer CreatePayloadBuffer<TRtmpChunkMessageHeader>(ref TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = _netBufferPool.Obtain();
            payloadWriter.Invoke(payloadBuffer);
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);
            return payloadBuffer;
        }
    }
}
