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
        private readonly IRtmpClientContext _clientContext;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IRtmpChunkMessageWriterService _writer;

        public RtmpChunkMessageSenderService(
            IRtmpClientContext clientContext,
            IDataBufferPool dataBufferPool,
            IRtmpChunkMessageWriterService writer)
        {
            _clientContext = clientContext;
            _dataBufferPool = dataBufferPool;
            _writer = writer;
        }

        public void Send<TRtmpChunkMessageHeader>(
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter,
            Action<bool>? callback)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);
            DoSend(basicHeader, messageHeader, callback, payloadBuffer);
        }

        public void Send<TRtmpChunkMessageHeader>(
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            IRentedBuffer payload,
            Action<bool>? callback = null)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payload);
            DoSend(basicHeader, messageHeader, callback, payloadBuffer);
        }

        private void DoSend<TRtmpChunkMessageHeader>(
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<bool>? callback,
            IDataBuffer payloadBuffer)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            try
            {
                var context = GetSessionContext();

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

        public ValueTask SendAsync<TRtmpChunkMessageHeader>(
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new ValueTaskCompletionSource();
            Send(basicHeader, messageHeader, payloadWriter, _ => tcs.SetResult());
            return tcs.Task;
        }

        public ValueTask SendAsync<TRtmpChunkMessageHeader>(
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            IRentedBuffer payload)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new ValueTaskCompletionSource();
            Send(basicHeader, messageHeader, payload, _ => tcs.SetResult());
            return tcs.Task;
        }

        private IDataBuffer CreatePayloadBuffer<TRtmpChunkMessageHeader>(
            ref TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = _dataBufferPool.Obtain();
            payloadWriter.Invoke(payloadBuffer);
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);
            return payloadBuffer;
        }

        private IDataBuffer CreatePayloadBuffer<TRtmpChunkMessageHeader>(
            ref TRtmpChunkMessageHeader messageHeader,
            IRentedBuffer payload)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = _dataBufferPool.Obtain();
            payloadBuffer.Write(payload.AsSpan());
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);
            return payloadBuffer;
        }

        private IRtmpSessionContext GetSessionContext()
        {
            return _clientContext.SessionContext ??
                throw new InvalidOperationException("Session context is not available");
        }
    }
}