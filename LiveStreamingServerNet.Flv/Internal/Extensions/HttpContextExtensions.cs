using Microsoft.AspNetCore.Http;


namespace LiveStreamingServerNet.Flv.Internal.Extensions
{
    internal static class HttpContextExtensions
    {
        public static bool ValidateNoEndpointDelegate(this HttpContext context)
            => context.GetEndpoint()?.RequestDelegate is null;

        public static bool ValidateGetOrHeadMethod(this HttpContext context)
            => HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
    }
}
