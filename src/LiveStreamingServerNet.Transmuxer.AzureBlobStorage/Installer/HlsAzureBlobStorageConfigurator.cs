using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Contracts;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer
{
    internal class HlsAzureBlobStorageConfigurator : IHlsAzureStorageConfigurator
    {
        public IServiceCollection Services { get; }

        public HlsAzureBlobStorageConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>()
            where TBlobPathResolver : class, IHlsAzureBlobPathResolver
        {
            Services.AddSingleton<IHlsAzureBlobPathResolver, TBlobPathResolver>();
            return this;
        }

        public IHlsAzureStorageConfigurator UseBlobPathResolver<TBlobPathResolver>(Func<IServiceProvider, TBlobPathResolver> implementationFactory)
            where TBlobPathResolver : class, IHlsAzureBlobPathResolver
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }
    }
}
