﻿namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null);

        ValueTask SendCommandMessageAsync(
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}