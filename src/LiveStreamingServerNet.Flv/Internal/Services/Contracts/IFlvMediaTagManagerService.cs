﻿using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagManagerService
    {
        ValueTask EnqueueMediaTagAsync(
            IFlvStreamContext streamContext,
            IList<IFlvClient> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IRentedBuffer rentedBuffer);

        ValueTask CacheSequenceHeaderAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            byte[] sequenceHeader);

        ValueTask CachePictureAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            IRentedBuffer rentedBuffer,
            uint timestamp);

        ValueTask ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext);

        ValueTask SendCachedHeaderTagsAsync(
            IFlvClient clientContext,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        ValueTask SendCachedMetaDataTagAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        ValueTask SendCachedGroupOfPicturesTagsAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            CancellationToken cancellation);

        void RegisterClient(IFlvClient client);
        void UnregisterClient(IFlvClient client);
    }
}
