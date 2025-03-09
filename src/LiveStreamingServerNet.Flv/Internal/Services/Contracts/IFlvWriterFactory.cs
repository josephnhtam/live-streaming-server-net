using LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvWriterFactory
    {
        IFlvWriter Create(IStreamWriter streamWriter);
    }
}
