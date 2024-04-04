using LiveStreamingServerNet.Transmuxer.Azure.Contracts;
using LiveStreamingServerNet.Transmuxer.Azure.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Azure.Installer
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
