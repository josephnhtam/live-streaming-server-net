using KubeOps.Operator;
using LiveStreamingServerNet.Operator.Installers;

namespace LiveStreamingServerNet.Operator
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddKubernetes()
                            .AddControllerServices();

            builder.Services.AddKubernetesOperator()
                            .RegisterComponents();

            var app = builder.Build();

            app.UseHealthChecks("/healthz");

            await app.RunAsync();
        }
    }
}
