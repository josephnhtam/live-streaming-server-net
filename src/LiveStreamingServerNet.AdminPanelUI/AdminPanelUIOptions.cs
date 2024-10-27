using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace LiveStreamingServerNet.AdminPanelUI
{
    /// <summary>
    /// Configuration options for the Admin Panel UI.
    /// </summary>
    public class AdminPanelUIOptions
    {
        /// <summary>
        /// Base path for the admin panel UI. 
        /// Default: "/ui".
        /// </summary>
        public string BasePath { get; set; } = "/ui";

        /// <summary>
        /// Base URI for stream-related API endpoints. 
        /// Default: "/api/v1/streams".
        /// </summary>
        public string StreamsBaseUri { get; set; } = "/api/v1/streams";

        /// <summary>
        /// Indicates whether HTTP FLV preview functionality is enabled.
        /// </summary>
        public bool HasHttpFlvPreview { get; set; }

        /// <summary>
        /// URI pattern for HTTP FLV streams. 
        /// Default: "{streamPath}.flv".
        /// </summary>
        public string HttpFlvUriPattern { get; set; } = "{streamPath}.flv";

        /// <summary>
        /// File provider for serving static files.
        /// </summary>
        public IFileProvider? FileProvider { get; set; }

        /// <summary>
        /// Provider for determining content types of served files.
        /// </summary>
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
