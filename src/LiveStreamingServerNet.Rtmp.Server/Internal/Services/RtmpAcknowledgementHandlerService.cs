using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpAcknowledgementHandlerService : IRtmpAcknowledgementHandlerService
    {
        private readonly IRtmpProtocolControlService _protocolControl;

        public RtmpAcknowledgementHandlerService(IRtmpProtocolControlService protocolControl)
        {
            _protocolControl = protocolControl;
        }

        public void Handle(IRtmpClientSessionContext clientContext, int receivedBytes)
        {
            if (clientContext.InWindowAcknowledgementSize == 0)
                return;

            clientContext.SequenceNumber += (uint)receivedBytes;
            if (clientContext.SequenceNumber - clientContext.LastAcknowledgedSequenceNumber >= clientContext.InWindowAcknowledgementSize)
            {
                _protocolControl.Acknowledgement(clientContext, clientContext.SequenceNumber);

                const uint overflow = 0xf0000000;
                if (clientContext.SequenceNumber >= overflow)
                {
                    clientContext.SequenceNumber -= overflow;
                    clientContext.LastAcknowledgedSequenceNumber -= overflow;
                }

                clientContext.LastAcknowledgedSequenceNumber = clientContext.SequenceNumber;
            }
        }
    }
}
