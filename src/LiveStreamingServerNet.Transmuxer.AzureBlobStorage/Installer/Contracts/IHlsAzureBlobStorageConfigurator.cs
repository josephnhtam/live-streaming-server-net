using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer.Contracts
{
    public interface IHlsAzureBlobStorageConfigurator
    {
        IServiceCollection Services { get; }
        IHlsAzureBlobStorageConfigurator UseBlobPathResolver<TBlobPathResolver>()
            where TBlobPathResolver : class, IHlsBlobPathResolver;
        IHlsAzureBlobStorageConfigurator UseBlobPathResolver<TBlobPathResolver>(Func<IServiceProvider, TBlobPathResolver> implementationFactory)
            where TBlobPathResolver : class, IHlsBlobPathResolver;
    }
}
