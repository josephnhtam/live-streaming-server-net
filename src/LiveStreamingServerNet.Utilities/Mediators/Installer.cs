using LiveStreamingServerNet.Utilities.Mediators.Contracts;
using LiveStreamingServerNet.Utilities.Mediators.Internal;
using LiveStreamingServerNet.Utilities.Mediators.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Utilities.Mediators
{
    public static class Installer
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, Action<IMediatorConfigurator> configure)
        {
            var configurator = new MediatorConfigurator();
            configure.Invoke(configurator);

            var configuration = configurator.Build();

            services.TryAddSingleton<IMediator, Mediator>();
            AddRequestHandlers(services, configuration);

            return services;
        }

        private static void AddRequestHandlers(IServiceCollection services, MediatorConfiguration configuration)
        {
            foreach (var (requestType, relavantTypesMap) in configuration.RequestRelavantTypesMap)
            {
                services.AddSingleton(
                    typeof(IRequestHandler<,>).MakeGenericType(requestType, relavantTypesMap.ResponseType),
                    relavantTypesMap.HandlerType);

                services.AddSingleton(relavantTypesMap.WrapperType);
            }

            var wrapperMap = configuration.RequestRelavantTypesMap.ToDictionary(
                x => x.Key,
                x => x.Value.WrapperType
            );

            services.AddSingleton<IRequestHandlerWrapperMap>(new RequestHandlerWrapperMap(wrapperMap));
        }
    }
}
