using Microsoft.AspNetCore.Builder;

namespace LiveStreamingServerNet.AdminPanelUI
{
    public static class Extensions
    {
        public static WebApplication UseAdminPanelUI(this WebApplication app, AdminPanelUIOptions? options = null)
        {
            app.UseMiddleware<AdminPanelUIMiddleware>(options ?? new AdminPanelUIOptions());
            return app;
        }
    }
}
