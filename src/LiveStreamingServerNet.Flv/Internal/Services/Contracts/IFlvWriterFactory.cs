using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvWriterFactory
    {
        IFlvWriter Create(IStreamWriter streamWriter);
    }
}
