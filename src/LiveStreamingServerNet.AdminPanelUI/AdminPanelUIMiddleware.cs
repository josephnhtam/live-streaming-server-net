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
            if (Validate(context) && await TryServeAdminPanelUI(context))
                return;

            await _next.Invoke(context);
        }

        private async Task<bool> TryServeAdminPanelUI(HttpContext context)
        {
            var fileContext = new AdminPanelUIFileContext(_options);
            return await fileContext.ServeAdminPanelUI(context);
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
