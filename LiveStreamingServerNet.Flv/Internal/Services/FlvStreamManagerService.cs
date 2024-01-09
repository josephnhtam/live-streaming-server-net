using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvStreamManagerService : IFlvStreamManagerService
    {
        public bool IsStreamPathPublishing(string publishStreamPath)
        {
            return false;
        }

        public PublishingStreamResult StartPublishingStream(string streamPath, IDictionary<string, string> streamArguments)
        {
            return PublishingStreamResult.Succeeded;
        }

        public bool StopPublishingStream(string streamPath, out IList<IFlvClient> existingSubscribers)
        {
            existingSubscribers = null!;
            return true;
        }

        public SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath)
        {
            return SubscribingStreamResult.Succeeded;
        }

        public bool StopSubscribingStream(IFlvClient client)
        {
            return true;
        }
    }
}
