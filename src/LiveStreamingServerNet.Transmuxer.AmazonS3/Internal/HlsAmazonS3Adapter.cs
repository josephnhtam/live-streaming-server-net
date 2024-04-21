using Amazon.S3;
using Amazon.S3.Model;
using LiveStreamingServerNet.Transmuxer.AmazonS3.Configurations;
using LiveStreamingServerNet.Transmuxer.AmazonS3.Internal.Logging;
using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LiveStreamingServerNet.Transmuxer.AmazonS3.Internal
{
    internal class HlsAmazonS3Adapter : IHlsStorageAdapter
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucket;
        private readonly HlsAmazonS3Configuration _config;
        private readonly ILogger _logger;

        public HlsAmazonS3Adapter(
            IAmazonS3 s3Client,
            string bucket,
            HlsAmazonS3Configuration config,
            ILogger<HlsAmazonS3Adapter> logger)
        {
            _s3Client = s3Client;
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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, tsFileName);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        FilePath = tsFilePath
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken);

                    return new StoredTsFile(tsFileName, _config.ObjectUriResolver.ResolveObjectUri(_bucket, objectPath));
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

                    var objectPath = _config.ObjectPathResolver.ResolveObjectPath(context, name);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = objectPath,
                        InputStream = contentStream
                    };

                    var response = await _s3Client.PutObjectAsync(request, cancellationToken);

                    return new StoredManifest(name, _config.ObjectUriResolver.ResolveObjectUri(_bucket, objectPath));
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

                    await _s3Client.DeleteObjectAsync(_bucket, objectPath, cancellationToken: cancellationToken);
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
