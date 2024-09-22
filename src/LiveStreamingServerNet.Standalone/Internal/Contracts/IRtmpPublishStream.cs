﻿using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Contracts
{
    internal interface IRtmpPublishStream
    {
        string Id { get; }
        ISessionControl Client { get; }
        string StreamPath { get; }
        DateTime StartTime { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? MetaData { get; }
        IReadOnlyList<ISessionControl> Subscribers { get; }
        int SubscribersCount { get; }
    }
}
