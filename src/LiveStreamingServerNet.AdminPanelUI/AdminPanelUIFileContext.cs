using Microsoft.AspNetCore.Http;
using System.Text;

namespace LiveStreamingServerNet.AdminPanelUI
{
    internal struct AdminPanelUIFileContext
    {
        private readonly AdminPanelUIOptions _options;

        public AdminPanelUIFileContext(AdminPanelUIOptions options)
        {
            ArgumentNullException.ThrowIfNull(options.FileProvider);
            ArgumentNullException.ThrowIfNull(options.ContentTypeProvider);

            _options = options;
        }

        public async Task<bool> ServeAdminPanelUI(HttpContext context)
        {
            if (await TryServeEnvConfig(context).ConfigureAwait(false))
                return true;

            if (await TryServeFile(context).ConfigureAwait(false))
                return true;

            if (await TryServeIndex(context).ConfigureAwait(false))
                return true;

            return false;
        }

        private async Task<bool> TryServeEnvConfig(HttpContext context)
        {
            var subPath = context.Request.Path.ToString();

            if (subPath != $"{AdminPanelUIConstants.FileBasePath}/env-config.js")
                return false;

            var envConfig = CreateEnvConfig();
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/javascript";
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(envConfig)).ConfigureAwait(false);

            return true;
        }

        private async Task<bool> TryServeFile(HttpContext context)
        {
            var subPath = context.Request.Path;

            if (!context.Request.Path.StartsWithSegments(AdminPanelUIConstants.FileBasePath))
                return false;

            return await TryServeFile(context, subPath.ToString().Substring(AdminPanelUIConstants.FileBasePath.Length)).ConfigureAwait(false);
        }

        private async Task<bool> TryServeIndex(HttpContext context)
        {
            if (_options.BasePath != "/" && !context.Request.Path.StartsWithSegments(_options.BasePath))
                return false;

            return await TryServeFile(context, "/index.html").ConfigureAwait(false);
        }

        private async Task<bool> TryServeFile(HttpContext context, PathString subPath)
        {
            if (!_options.ContentTypeProvider!.TryGetContentType(subPath, out var contentType))
                return false;

            var fileInfo = _options.FileProvider!.GetFileInfo(subPath);

            if (!fileInfo.Exists)
                return false;

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;
            await context.Response.SendFileAsync(fileInfo).ConfigureAwait(false);

            return true;
        }

        private string CreateEnvConfig()
        {
            var builder = new StringBuilder();

            builder.Append("window._env = {");

            foreach (var (key, value) in _options.GetParameters())
            {
                switch (value)
                {
                    case bool boolValue:
                        builder.Append($"{key}:{boolValue.ToString().ToLower()},");
                        break;
                    case string stringValue:
                        builder.Append($"{key}:\"{stringValue}\",");
                        break;
                    default:
                        builder.Append($"{key}:{value},");
                        break;
                }
            }

            builder.Append("}");

            return builder.ToString();
        }
    }
}
