﻿using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpCommandMessageSenderService : IRtmpCommandMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpCommandMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public void SendCommandMessage(
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters,
            AmfEncodingType amfEncodingType,
            Action<bool>? callback)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, messageStreamId);

            _chunkMessageSender.Send(basicHeader, messageHeader, dataBuffer =>
            {
                var values = CreateParameters(commandName, transactionId, commandObject, parameters);
                dataBuffer.WriteAmf(values, amfEncodingType);
            }, callback);
        }

        public ValueTask SendCommandMessageAsync(
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new ValueTaskCompletionSource();
            SendCommandMessage(messageStreamId, chunkStreamId, commandName, transactionId, commandObject, parameters, amfEncodingType, _ => tcs.SetResult());
            return tcs.Task;
        }

        static List<object?> CreateParameters(string commandName, double transactionId,
            IReadOnlyDictionary<string, object>? commandObject, IReadOnlyList<object?>? parameters)
        {
            var result = new List<object?>
            {
                commandName,
                transactionId,
                commandObject
            };

            if (parameters != null)
                result.AddRange(parameters);

            return result;
        }
    }
}