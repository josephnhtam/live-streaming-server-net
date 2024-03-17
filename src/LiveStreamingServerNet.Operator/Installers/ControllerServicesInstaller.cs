using LiveStreamingServerNet.Operator.Services.Contracts;
using LiveStreamingServerNet.Operator.Services;

namespace LiveStreamingServerNet.Operator.Installers
{
    public static class ControllerServicesInstaller
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddSingleton<IClusterStateRetriver, ClusterStateRetriver>()
                    .AddSingleton<IDesiredStateCalculator, DesiredStateCalculator>()
                    .AddSingleton<IDesiredStateStabilizer, DesiredStateStabilizer>()
                    .AddSingleton<IDesiredStateApplier, DesiredStateApplier>();
            return services;
        }
    }
}
