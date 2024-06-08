using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using System.Web;

namespace LiveStreamingServerNet.StreamProcessor
{
    public class DefaultInputPathResolver : IInputPathResolver
    {
        private readonly IServerHandle _server;
        private readonly IRtmpServerContext _serverContext;

        public DefaultInputPathResolver(IServerHandle server, IRtmpServerContext serverContext)
        {
            _server = server;
            _serverContext = serverContext;
        }

        public Task<string> ResolveInputPathAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var inputUri = new Uri(GetLocalServerBaseUri(), streamPath);
            return Task.FromResult(AddAuthCode(inputUri));
        }

        private string AddAuthCode(Uri inputUri)
        {
            return $"{inputUri}?code={HttpUtility.UrlEncode(_serverContext.AuthCode)}";
        }

        private Uri GetLocalServerBaseUri()
        {
            var localServerEndPoint = GetLocalServerEndPoint();
            var scheme = localServerEndPoint.IsSecure ? "rtmps" : "rtmp";
            return new Uri($"{scheme}://localhost:{localServerEndPoint.LocalEndPoint.Port}");
        }

        private ServerEndPoint GetLocalServerEndPoint()
        {
            if (!_server.IsStarted || !(_server.EndPoints?.Any() ?? false))
                throw new InvalidOperationException("Server is not running locally.");

            return _server.EndPoints.FirstOrDefault(x => !x.IsSecure) ?? _server.EndPoints.First();
        }
    }
}
