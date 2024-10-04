using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpAcknowledgementHandlerService : IRtmpAcknowledgementHandlerService
    {
        private readonly IRtmpProtocolControlService _protocolControl;

        public RtmpAcknowledgementHandlerService(IRtmpProtocolControlService protocolControl)
        {
            _protocolControl = protocolControl;
        }

        public void Handle(IRtmpSessionContext context, int receivedBytes)
        {
            if (context.InWindowAcknowledgementSize == 0)
                return;

            context.SequenceNumber += (uint)receivedBytes;
            if (context.SequenceNumber - context.LastAcknowledgedSequenceNumber >= context.InWindowAcknowledgementSize)
            {
                _protocolControl.Acknowledgement(context.SequenceNumber);

                const uint overflow = 0xf0000000;
                if (context.SequenceNumber >= overflow)
                {
                    context.SequenceNumber -= overflow;
                    context.LastAcknowledgedSequenceNumber -= overflow;
                }

                context.LastAcknowledgedSequenceNumber = context.SequenceNumber;
            }
        }
    }
}
