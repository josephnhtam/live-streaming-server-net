using LiveStreamingServerNet.Standalone.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Standalone.EndpointFilters
{
    public class ApiExceptionEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                return await next.Invoke(context);
            }
            catch (ApiException ex)
            {
                return Results.Json(ex.ErrorResponse, statusCode: ex.StatusCode);
            }
        }
    }
}
