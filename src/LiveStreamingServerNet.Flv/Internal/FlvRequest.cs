using LiveStreamingServerNet.Flv.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Flv.Internal
{
    public class FlvRequest : IFlvRequest
    {
        public string Scheme { get; }
        public bool IsHttps { get; }
        public HostString Host { get; }
        public PathString PathBase { get; set; }
        public PathString Path { get; }
        public QueryString QueryString { get; }
        public IQueryCollection Query { get; }
        public string Protocol { get; set; }
        public IReadOnlyDictionary<string, StringValues> Headers { get; }
        public IRequestCookieCollection Cookies { get; }
        public IReadOnlyDictionary<string, object?> RouteValues { get; }

        public IPAddress? RemoteIpAddress { get; }
        public int RemotePort { get; set; }
        public IPAddress? LocalIpAddress { get; }
        public int LocalPort { get; }
        public X509Certificate2? ClientCertificate { get; }

        public FlvRequest(HttpContext context)
        {
            Scheme = context.Request.Scheme;
            IsHttps = context.Request.IsHttps;
            Host = context.Request.Host;
            PathBase = context.Request.PathBase;
            Path = context.Request.Path;
            QueryString = context.Request.QueryString;
            Query = context.Request.Query;
            Protocol = context.Request.Protocol;
            Headers = context.Request.Headers.AsReadOnly();
            Cookies = context.Request.Cookies;
            RouteValues = context.Request.RouteValues;

            var connection = context.Connection;
            RemoteIpAddress = connection.RemoteIpAddress;
            RemotePort = connection.RemotePort;
            LocalIpAddress = connection.LocalIpAddress;
            LocalPort = connection.LocalPort;
            ClientCertificate = connection.ClientCertificate;
        }
    }
}
