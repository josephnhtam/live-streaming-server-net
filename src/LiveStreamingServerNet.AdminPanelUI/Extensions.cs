using Microsoft.AspNetCore.Builder;

namespace LiveStreamingServerNet.AdminPanelUI
{
    /// <summary>
    /// Contains extension methods for WebApplication configurations
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds the Admin Panel UI middleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The WebApplication instance to configure.</param>
        /// <returns>The WebApplication instance for method chaining.</returns>
        public static WebApplication UseAdminPanelUI(this WebApplication app)
            => UseAdminPanelUI(app, new AdminPanelUIOptions());

        /// <summary>
        /// Adds the Admin Panel UI middleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The WebApplication instance to configure.</param>
        /// <param name="options">Optional configuration options for the Admin Panel UI.</param>
        /// <returns>The WebApplication instance for method chaining.</returns>
        public static WebApplication UseAdminPanelUI(this WebApplication app, AdminPanelUIOptions options)
        {
            app.UseMiddleware<AdminPanelUIMiddleware>(options);
            return app;
        }
    }
}
