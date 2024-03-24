using LiveStreamingServerNet.KubernetesOperator.Services;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Installers
{
    public static class ControllerServicesInstaller
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddSingleton<IFleetStateRetriver, FleetStateRetriver>()
                    .AddSingleton<IDesiredStateCalculator, DesiredStateCalculator>()
                    .AddSingleton<IDesiredStateApplier, DesiredStateApplier>()
                    .AddSingleton<IPodCleaner, PodCleaner>()
                    .AddSingleton<IFleetScaler, FleetScaler>()
                    .AddTransient<ITargetReplicasStabilizer, TargetReplicasStabilizer>();

            return services;
        }
    }
}
