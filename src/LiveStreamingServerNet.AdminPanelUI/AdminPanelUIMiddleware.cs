using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.AdminPanelUI
{
    public class AdminPanelUIMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AdminPanelUIOptions _options;

        public AdminPanelUIMiddleware(RequestDelegate next, AdminPanelUIOptions? options)
        {
            _next = next;
            _options = options ?? new AdminPanelUIOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next.Invoke(context);

            if (Validate(context))
                await TryServeAdminPanelUI(context);
        }

        private async Task TryServeAdminPanelUI(HttpContext context)
        {
            var fileContext = new AdminPanelUIFileContext(_next, _options);
            await fileContext.ServeAdminPanelUI(context);
        }

        private bool Validate(HttpContext context)
        {
            if (context.GetEndpoint() != null)
                return false;

            if (context.Response.ContentLength != null)
                return false;

            return true;
        }
    }
}
