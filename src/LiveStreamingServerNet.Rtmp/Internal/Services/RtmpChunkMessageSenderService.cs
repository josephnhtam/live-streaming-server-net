using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpChunkMessageSenderService : IRtmpChunkMessageSenderService
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpChunkMessageWriterService _writer;

        public RtmpChunkMessageSenderService(IDataBufferPool dataBufferPool, IRtmpChunkMessageWriterService writer)
        {
            _dataBufferPool = dataBufferPool;
            _writer = writer;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter,
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
                _dataBufferPool.Recycle(payloadBuffer);
            }
        }

        public ValueTask SendAsync<TRtmpChunkMessageHeader>(IRtmpClientContext clientContext, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<IDataBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new ValueTaskCompletionSource();
            Send(clientContext, basicHeader, messageHeader, payloadWriter, _ => tcs.SetResult());
            return tcs.Task;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IReadOnlyList<IRtmpClientContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
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

                    var targetBuffer = _dataBufferPool.Obtain();

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
                        _dataBufferPool.Recycle(targetBuffer);
                    }
                }
            }
            finally
            {
                _dataBufferPool.Recycle(payloadBuffer);
            }
        }

        private IDataBuffer CreatePayloadBuffer<TRtmpChunkMessageHeader>(ref TRtmpChunkMessageHeader messageHeader, Action<IDataBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = _dataBufferPool.Obtain();
            payloadWriter.Invoke(payloadBuffer);
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);
            return payloadBuffer;
        }
    }
}
