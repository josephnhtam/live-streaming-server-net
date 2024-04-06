using LiveStreamingServerNet.KubernetesOperator.Services;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Installer
{
    public static class ControllerServicesInstaller
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            services.AddSingleton<IFleetScalerResolver, FleetScalerResolver>()
                    .AddScoped<IFleetStateFetcher, FleetStateFetcher>()
                    .AddScoped<IDesiredFleetStateCalculator, DesiredFleetStateCalculator>()
                    .AddScoped<IDesiredFleetStateApplier, DesiredFleetStateApplier>()
                    .AddScoped<IPodCleaner, PodCleaner>()
                    .AddScoped<IFleetScaler, FleetScaler>()
                    .AddScoped<IPodTemplateCreator, PodTemplateCreator>()
                    .AddTransient<ITargetReplicasStabilizer, TargetReplicasStabilizer>();

            return services;
        }
    }
}
