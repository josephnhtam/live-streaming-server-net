using k8s;

namespace LiveStreamingServerNet.KubernetesOperator.Installer
{
    public static class KubernetesInstaller
    {
        public static IServiceCollection AddKubernetes(this IServiceCollection services)
        {
            services.AddSingleton(CreateKubernetesClient);
            return services;
        }

        private static IKubernetes CreateKubernetesClient(IServiceProvider provider)
        {
            var kubeConfig = KubernetesClientConfiguration.InClusterConfig();
            return new Kubernetes(kubeConfig);
        }
    }
}
