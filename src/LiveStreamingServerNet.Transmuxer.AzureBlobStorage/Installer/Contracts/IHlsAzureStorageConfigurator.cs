using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer.Contracts
{
    public interface IHlsAzureStorageConfigurator
    {
        IServiceCollection Services { get; }
        IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>()
            where TBlobPathResolver : class, IHlsBlobPathResolver;
        IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>(Func<IServiceProvider, TBlobPathResolver> implementationFactory)
            where TBlobPathResolver : class, IHlsBlobPathResolver;
    }
}
