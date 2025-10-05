using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.Flv.Contracts
{
    public interface IFlvRequest
    {
        /// <summary>
        /// Gets or sets the HTTP request scheme.
        /// </summary>
        /// <returns>The HTTP request scheme.</returns>
        string Scheme { get; }

        /// <summary>
        /// Returns true if the RequestScheme is https.
        /// </summary>
        /// <returns>true if this request is using https; otherwise, false.</returns>
        bool IsHttps { get; }

        /// <summary>
        /// Gets the Host header. May include the port.
        /// </summary>
        /// <return>The Host header.</return>
        HostString Host { get; }

        /// <summary>
        /// Gets the base path for the request. The path base should not end with a trailing slash.
        /// </summary>
        /// <returns>The base path for the request.</returns>
        PathString PathBase { get; set; }

        /// <summary>
        /// Gets or sets the request path from RequestPath.
        /// </summary>
        /// <returns>The request path from RequestPath.</returns>
        PathString Path { get; }

        /// <summary>
        /// Gets the raw query string used to create the query collection in Request.Query.
        /// </summary>
        /// <returns>The raw query string.</returns>
        QueryString QueryString { get; }

        /// <summary>
        /// Gets the query value collection parsed from Request.QueryString.
        /// </summary>
        /// <returns>The query value collection parsed from Request.QueryString.</returns>
        IQueryCollection Query { get; }

        /// <summary>
        /// Gets or sets the request protocol (e.g. HTTP/1.1).
        /// </summary>
        /// <returns>The request protocol.</returns>
        string Protocol { get; set; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        /// <returns>The request headers.</returns>
        IReadOnlyDictionary<string, StringValues> Headers { get; }

        /// <summary>
        /// Gets the collection of Cookies for this request.
        /// </summary>
        /// <returns>The collection of Cookies for this request.</returns>
        IRequestCookieCollection Cookies { get; }

        /// <summary>
        /// Gets the collection of route values for this request.
        /// </summary>
        /// <returns>The collection of route values for this request.</returns>
        IReadOnlyDictionary<string, object?> RouteValues { get; }

        /// <summary>
        /// Gets the IP address of the remote target. Can be null.
        /// </summary>
        IPAddress? RemoteIpAddress { get; }

        /// <summary>
        /// Gets the port of the remote target.
        /// </summary>
        int RemotePort { get; set; }

        /// <summary>
        /// Gets the IP address of the local host.
        /// </summary>
        IPAddress? LocalIpAddress { get; }

        /// <summary>
        /// Gets the port of the local host.
        /// </summary>
        int LocalPort { get; }

        /// <summary>
        /// Gets the client certificate.
        /// </summary>
        X509Certificate2? ClientCertificate { get; }
    }
}
