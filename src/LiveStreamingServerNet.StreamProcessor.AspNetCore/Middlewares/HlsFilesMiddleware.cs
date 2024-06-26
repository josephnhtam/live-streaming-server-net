﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.StreamProcessor.AspNetCore.Middlewares
{
    public class HlsFilesMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly IHlsPathMapper _pathMapper;
        private readonly HlsServingOptions _options;
        private readonly PhysicalFileProvider _fileProvider;
        private readonly StaticFileMiddleware _staticFile;

        private static readonly PathString _hlsServingPath = $"/hls-serving/{Guid.NewGuid()}";

        public HlsFilesMiddleware(
            RequestDelegate next,
            IServer server,
            IWebHostEnvironment hostingEnv,
            IOptions<HlsServingOptions> options,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _pathMapper = server.Services.GetRequiredService<IHlsPathMapper>();
            _options = options.Value;

            DirectoryUtility.CreateDirectoryIfNotExists(_options.Root);
            _fileProvider = new PhysicalFileProvider(_options.Root);

            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = _fileProvider,
                ContentTypeProvider = CreateContentTypeProvider(),
                HttpsCompression = _options.HttpsCompression,
                RequestPath = _hlsServingPath,
                OnPrepareResponse = _options.OnPrepareResponse,
#if NET8_0_OR_GREATER
                OnPrepareResponseAsync = _options.OnPrepareResponseAsync
#endif
            };

            _staticFile = new StaticFileMiddleware(Next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        private static FileExtensionContentTypeProvider CreateContentTypeProvider()
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".m3u8"] = "application/x-mpegURL";

            return contentTypeProvider;
        }

        private Task Next(HttpContext context)
        {
            if (context.Items.TryGetValue("originalPath", out var path) && path is PathString originalPath)
            {
                context.Request.Path = originalPath;
            }

            return _next.Invoke(context);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ValidateNoEndpoint(context) || !ValidateMethod(context) || !ValidatePath(context, _options.RequestPath, out var fileSubPath))
            {
                await _next.Invoke(context);
                return;
            }

            context.Items["originalPath"] = context.Request.Path;
            context.Request.Path = _hlsServingPath.Add(fileSubPath);

            await _staticFile.Invoke(context);
        }

        private PathString GetFileSubPath(PathString subPath)
        {
            var streamPath = Path.GetDirectoryName(subPath.Value) ?? string.Empty;
            var outputPath = _pathMapper.GetHlsOutputPath(streamPath);

            if (string.IsNullOrEmpty(outputPath))
                return PathString.Empty;

            try
            {
                var filePath = Path.Combine(outputPath, Path.GetFileName(subPath.Value) ?? string.Empty);
                return new PathString($"/{Path.GetRelativePath(_options.Root, filePath).Replace('\\', '/').Trim('/')}");
            }
            catch
            {
                return PathString.Empty;
            }
        }

        private bool ValidatePath(HttpContext context, PathString matchUrl, out PathString fileSubPath)
        {
            if (!context.Request.Path.StartsWithSegments(matchUrl, out var subPath))
            {
                fileSubPath = PathString.Empty;
                return false;
            }

            fileSubPath = GetFileSubPath(subPath);
            return fileSubPath.HasValue;
        }

        private static bool ValidateNoEndpoint(HttpContext httpContext)
        {
            return httpContext.GetEndpoint() == null;
        }

        private static bool ValidateMethod(HttpContext httpContext)
        {
            var method = httpContext.Request.Method;
            return HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
        }

        public void Dispose()
        {
            _fileProvider.Dispose();
        }
    }
}
