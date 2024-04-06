using Google.Cloud.Storage.V1;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Configurations;
using LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Internal.Logging;
using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LiveStreamingServerNet.Transmuxer.GoogleCloudStorage.Internal
{
    public class HlsGoogleCloudStorageAdapter : IHlsStorageAdapter
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucket;
        private readonly HlsGoogleCloudStorageConfiguration _config;
        private readonly ILogger _logger;

        public HlsGoogleCloudStorageAdapter(
            StorageClient storageClient,
            string bucket,
            HlsGoogleCloudStorageConfiguration config,
            ILogger<HlsGoogleCloudStorageAdapter> logger)
        {
            _storageClient = storageClient;
            _bucket = bucket;
            _config = config;
            _logger = logger;
        }

        public async Task<StoringResult> StoreAsync(
            TransmuxingContext context,
            IReadOnlyList<Manifest> manifests,
            IReadOnlyList<TsFile> tsFiles,
            CancellationToken cancellationToken)
        {
            var storedTsFiles = await UploadTsFilesAsync(context, tsFiles, cancellationToken);
            var storedManifestFiles = await UploadManifestFilesAsync(context, manifests, cancellationToken);
            return new StoringResult(storedManifestFiles, storedTsFiles);
        }

        private async Task<IReadOnlyList<StoredTsFile>> UploadTsFilesAsync(
            TransmuxingContext context,
            IReadOnlyList<TsFile> tsFiles,
            CancellationToken cancellationToken)
        {
            var dirPath = Path.GetDirectoryName(context.OutputPath) ?? string.Empty;

            var tasks = new List<Task<StoredTsFile>>();

            foreach (var tsFile in tsFiles)
            {
                var tsFilePath = Path.Combine(dirPath, tsFile.FileName);
                tasks.Add(UploadTsFileAsync(tsFile.FileName, tsFilePath, cancellationToken));
            }

            return await Task.WhenAll(tasks);

            async Task<StoredTsFile> UploadTsFileAsync
                (string tsFileName, string tsFilePath, CancellationToken cancellationToken)
            {
                try
                {
                    using var fileStream = File.OpenRead(tsFilePath);

                    var @object = new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _bucket,
                        Name = _config.ObjectPathResolver.ResolveObjectPath(context, tsFileName),
                        CacheControl = _config.TsFilesCacheControl
                    };

                    var response = await _storageClient.UploadObjectAsync(
                        @object, fileStream,
                        options: _config.TsFilesUploadObjectOptions,
                        cancellationToken: cancellationToken);

                    return new StoredTsFile(tsFileName, new Uri($"https://storage.googleapis.com/{response.Id}"));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.UploadingTsFileError(
                        context.Transmuxer, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, tsFilePath, ex);

                    return new StoredTsFile(tsFileName, null);
                }
            }
        }

        private async Task<IReadOnlyList<StoredManifest>> UploadManifestFilesAsync(
           TransmuxingContext context,
           IReadOnlyList<Manifest> manifests,
           CancellationToken cancellationToken)
        {
            var tasks = new List<Task<StoredManifest>>();

            foreach (var manifest in manifests)
            {
                tasks.Add(UploadManifestAsync(manifest.Name, manifest.Content, cancellationToken));
            }

            return await Task.WhenAll(tasks);

            async Task<StoredManifest> UploadManifestAsync
                (string name, string content, CancellationToken cancellationToken)
            {
                try
                {
                    using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                    var @object = new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _bucket,
                        Name = _config.ObjectPathResolver.ResolveObjectPath(context, name),
                        CacheControl = _config.ManifestsCacheControl
                    };

                    var response = await _storageClient.UploadObjectAsync(
                        @object, contentStream,
                        options: _config.ManifestsUploadObjectOptions,
                        cancellationToken: cancellationToken);

                    return new StoredManifest(name, new Uri($"https://storage.googleapis.com/{response.Id}"));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.UploadingManifestFileError(
                        context.Transmuxer, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, name, ex);

                    return new StoredManifest(name, null);
                }
            }
        }

        public async Task DeleteAsync(TransmuxingContext context, IReadOnlyList<TsFile> tsFiles, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var tsFile in tsFiles)
            {
                tasks.Add(DeleteTsFileAsync(tsFile.FileName, cancellationToken));
            }

            await Task.WhenAll(tasks);

            async Task DeleteTsFileAsync
                (string tsFileName, CancellationToken cancellationToken)
            {
                try
                {
                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, tsFileName);

                    await _storageClient.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.DeletingTsFileError(
                        context.Transmuxer, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
                }
            }
        }
    }
}
