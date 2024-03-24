using LiveStreamingServerNet.KubernetesOperator.Services;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Installer
{
    public static class ControllerServicesInstaller
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddSingleton<IFleetStateFetcher, FleetStateFetcher>()
                    .AddSingleton<IDesiredFleetStateCalculator, DesiredFleetStateCalculator>()
                    .AddSingleton<IDesiredFleetStateApplier, DesiredFleetStateApplier>()
                    .AddSingleton<IPodCleaner, PodCleaner>()
                    .AddSingleton<IFleetScaler, FleetScaler>()
                    .AddSingleton<IPodTemplateCreator, PodTemplateCreator>()
                    .AddTransient<ITargetReplicasStabilizer, TargetReplicasStabilizer>();

            return services;
        }
    }
}
