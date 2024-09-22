﻿using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mtanksl.ActionMessageFormat;
using System.Collections.Concurrent;
using System.Reflection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher
{
    internal class RtmpCommandDispatcher<TContext> : IRtmpCommandDispatcher<TContext>
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpCommandHanlderMap _handlerMap;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Type, (Type, ParameterInfo[])> _commandParametersMap;
        private readonly ConcurrentDictionary<Type, RtmpCommandHandler<TContext>> _commandHandlerCache;

        public RtmpCommandDispatcher(IServiceProvider services, IRtmpCommandHanlderMap handlerMap, ILogger<RtmpCommandDispatcher<TContext>> logger)
        {
            _services = services;
            _handlerMap = handlerMap;
            _logger = logger;
            _commandParametersMap = new ConcurrentDictionary<Type, (Type, ParameterInfo[])>();
            _commandHandlerCache = new ConcurrentDictionary<Type, RtmpCommandHandler<TContext>>();
        }

        public async ValueTask<bool> DispatchAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            TContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var isUsingAmf3 = IsUsingAmf3(chunkStreamContext);
            var bytes = payloadBuffer.ReadBytes(chunkStreamContext.MessageHeader.MessageLength);
            var reader = new AmfReader(bytes);

            var commandName = (string)(isUsingAmf3 ? reader.ReadAmf3() : reader.ReadAmf0());

            var commandHandlerType = _handlerMap.GetHandlerType(commandName);

            if (commandHandlerType == null)
                return true;

            var (commandType, commandParameterInfos) = GetCommandInfo(commandHandlerType);
            var commandParameters = ReadParameters(commandParameterInfos, reader, isUsingAmf3);
            var command = Activator.CreateInstance(commandType, commandParameters)!;
            var commandHandler = GetCommandHandler(commandHandlerType);

            return await commandHandler.HandleAsync(chunkStreamContext, context, command, cancellationToken);
        }

        private object[] ReadParameters(ParameterInfo[] commandParameterInfos, AmfReader reader, bool isUsingAmf3)
        {
            var results = new object[commandParameterInfos.Length];

            try
            {
                for (int i = 0; i < commandParameterInfos.Length; i++)
                {
                    var parameter = isUsingAmf3 ? reader.ReadAmf3() : reader.ReadAmf0();

                    if (parameter is IAmfObject amfObject)
                        parameter = amfObject.ToObject();

                    if (parameter is Dictionary<object, object> dictionary)
                        parameter = dictionary.ToDictionary(x => x.Key.ToString()!, x => x.Value);

                    results[i] = parameter;
                }
            }
            catch (IndexOutOfRangeException) { }

            return results;
        }

        private (Type, ParameterInfo[]) GetCommandInfo(Type handlerType)
        {
            return _commandParametersMap.GetOrAdd(handlerType, (handlerType) =>
            {
                var commandType = handlerType.GetGenericArguments(typeof(RtmpCommandHandler<,>))[0];
                var commandConstructor = commandType.GetConstructors().First();

                return (commandType, commandConstructor.GetParameters());
            });
        }

        private RtmpCommandHandler<TContext> GetCommandHandler(Type commandHandlerType)
        {
            return _commandHandlerCache.GetOrAdd(commandHandlerType, (commandHandlerType) =>
                (_services.GetRequiredService(commandHandlerType) as RtmpCommandHandler<TContext>)!
            );
        }

        private static bool IsUsingAmf3(IRtmpChunkStreamContext chunkStreamContext)
        {
            return chunkStreamContext.MessageHeader.MessageTypeId == RtmpMessageType.CommandMessageAmf3;
        }
    }
}
