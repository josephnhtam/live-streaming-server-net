using k8s;
using Polly;
using Polly.Retry;

namespace LiveStreamingServerNet.KubernetesOperator.Installer
{
    public static class ResiliencePipelineInstaller
    {
        public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
        {
            services.AddResiliencePipeline("k8s-pipeline", builder =>
                builder.AddConcurrencyLimiter(10, int.MaxValue)
                    .AddRetry(new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<KubernetesException>(),
                        BackoffType = DelayBackoffType.Exponential,
                        Delay = TimeSpan.FromMilliseconds(200),
                        MaxDelay = TimeSpan.FromSeconds(1)
                    })
            );

            return services;
        }
    }
}
