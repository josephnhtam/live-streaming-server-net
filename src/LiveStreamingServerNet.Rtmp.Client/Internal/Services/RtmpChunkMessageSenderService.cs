using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
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
            IRtmpSessionContext context,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter,
            Action<bool>? callback) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            try
            {
                context.Session.Send(targetBuffer =>
                    _writer.Write(targetBuffer, basicHeader, messageHeader, payloadBuffer, context.OutChunkSize),
                    callback
                );
            }
            finally
            {
                _dataBufferPool.Recycle(payloadBuffer);
            }
        }

        public ValueTask SendAsync<TRtmpChunkMessageHeader>(IRtmpSessionContext context, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<IDataBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new ValueTaskCompletionSource();
            Send(context, basicHeader, messageHeader, payloadWriter, _ => tcs.SetResult());
            return tcs.Task;
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