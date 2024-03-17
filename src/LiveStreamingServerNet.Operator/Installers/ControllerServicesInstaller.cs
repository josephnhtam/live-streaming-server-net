using LiveStreamingServerNet.Operator.Services.Contracts;
using LiveStreamingServerNet.Operator.Services;

namespace LiveStreamingServerNet.Operator.Installers
{
    public static class ControllerServicesInstaller
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddSingleton<IDesiredStateCalculator, DesiredStateCalculator>()
                    .AddSingleton<IClusterStateRetriver, ClusterStateRetriver>()
                    .AddSingleton<IDesiredStateStabilizer, DesiredStateStabilizer>();
            return services;
        }
    }
}
