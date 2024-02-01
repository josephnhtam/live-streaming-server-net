using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace LiveStreamingServerNet.AdminPanelUI
{
    public class AdminPanelUIOptions
    {
        public string BasePath { get; set; } = "/ui";
        public string StreamsBaseUri { get; set; } = "/api/v1/streams";
        public bool HasHttpFlvPreview { get; set; }
        public string HttpFlvUriPattern { get; set; } = "{streamPath}.flv";
        public IFileProvider? FileProvider { get; set; }
        public IContentTypeProvider? ContentTypeProvider { get; set; }

        public AdminPanelUIOptions()
        {
            var uiPath = GetDefaultAdminPanelUIPath();

            if (uiPath != null)
                FileProvider = new PhysicalFileProvider(uiPath);

            ContentTypeProvider = new FileExtensionContentTypeProvider();
        }

        private static string? GetDefaultAdminPanelUIPath()
        {
            var directoryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                var adminPanelUIPath = Path.Combine(directoryPath, "admin-panel-ui");

                if (Directory.Exists(adminPanelUIPath))
                    return adminPanelUIPath;
            }

            return null;
        }

        internal IDictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>()
            {
                ["BASE_PATH"] = BasePath,
                ["STREAMS_BASE_URI"] = StreamsBaseUri,
                ["HAS_HTTP_FLV_PREVIEW"] = HasHttpFlvPreview,
                ["HTTP_FLV_URI_PATTERN"] = HttpFlvUriPattern,
            };
        }
    }
}
