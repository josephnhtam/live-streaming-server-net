using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Events
{
    internal record RtmpStreamPublishedEvent(IRtmpPublishStream stream);
    internal record RtmpStreamUnpublishedEvent(IRtmpPublishStream stream);
    internal record RtmpStreamMetaDataReceivedEvent(IRtmpPublishStream stream);
    internal record RtmpStreamSubscribedEvent(IRtmpPublishStream stream, IClientControl subscriber);
    internal record RtmpStreamUnsubscribedEvent(IRtmpPublishStream stream, IClientControl subscriber);
}
