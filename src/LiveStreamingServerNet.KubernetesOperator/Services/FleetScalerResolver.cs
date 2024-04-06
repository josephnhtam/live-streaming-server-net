using k8s.Models;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class FleetScalerResolver : IFleetScalerResolver
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<string, IServiceScope> _entityScopes;

        public FleetScalerResolver(IServiceProvider services)
        {
            _services = services;
            _entityScopes = new Dictionary<string, IServiceScope>();
        }

        public IFleetScaler Resolve(V1LiveStreamingServerFleet entity)
        {
            return GetEntityScope(entity).ServiceProvider.GetRequiredService<IFleetScaler>();
        }

        public void Finalize(V1LiveStreamingServerFleet entity)
        {
            GetEntityScope(entity).Dispose();
        }

        private IServiceScope GetEntityScope(V1LiveStreamingServerFleet entity)
        {
            lock (_entityScopes)
            {
                string uid = entity.Uid()!;

                if (!_entityScopes.ContainsKey(uid))
                    _entityScopes[uid] = _services.CreateScope();

                return _entityScopes[uid];
            }
        }
    }
}
