using LiveStreamingServerNet.Flv.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace LiveStreamingServerNet.Flv
{
    public class DefaultStreamPathResolver : IStreamPathResolver
    {
        public bool ResolveStreamPathAndArguments(HttpContext context, out string streamPath, out IDictionary<string, string> streamArguments)
        {
            streamPath = default!;
            streamArguments = default!;

            var path = context.Request.Path.ToString().TrimEnd('/');
            var query = context.Request.QueryString.ToString();
            var extension = Path.GetExtension(path);

            if (path.Length <= 1 || !extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
                return false;

            streamPath = path.Substring(0, path.Length - 4);
            streamArguments = QueryHelpers.ParseQuery(query).ToDictionary(x => x.Key, x => x.Value.ToString());
            return true;
        }
    }
}
