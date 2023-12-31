﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientHandlerFactory : IClientHandlerFactory
    {
        private readonly IServiceProvider _services;

        public RtmpClientHandlerFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IClientHandler CreateClientHandler(IClientHandle client)
        {
            var handler = _services.GetRequiredService<IRtmpClientHandler>();
            handler.InitializeAsync(new RtmpClientContext(client));
            return handler;
        }
    }
}
