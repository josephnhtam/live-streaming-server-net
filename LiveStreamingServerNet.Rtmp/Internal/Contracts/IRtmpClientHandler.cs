﻿using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientHandler : IClientHandler
    {
        Task InitializeAsync(IRtmpClientContext clientContext);
    }
}
