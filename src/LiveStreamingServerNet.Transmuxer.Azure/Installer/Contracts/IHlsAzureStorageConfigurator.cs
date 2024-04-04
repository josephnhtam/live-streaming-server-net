using LiveStreamingServerNet.Transmuxer.Azure.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Azure.Installer.Contracts
{
    public interface IHlsAzureStorageConfigurator
    {
        IServiceCollection Services { get; }
        IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>()
            where TBlobPathResolver : class, IHlsAzureBlobPathResolver;
        IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>(Func<IServiceProvider, TBlobPathResolver> implementationFactory)
            where TBlobPathResolver : class, IHlsAzureBlobPathResolver;
    }
}
