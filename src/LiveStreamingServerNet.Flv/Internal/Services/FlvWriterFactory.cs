﻿using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvWriterFactory : IFlvWriterFactory
    {
        private readonly IDataBufferPool _dataBufferPool;

        public FlvWriterFactory(IDataBufferPool dataBufferPool)
        {
            _dataBufferPool = dataBufferPool;
        }

        public IFlvWriter Create(IStreamWriter streamWriter)
        {
            return new FlvWriter(streamWriter, _dataBufferPool);
        }
    }
}
