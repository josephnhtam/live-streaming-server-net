using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions
{
    internal static class RtmpCommandMessageSenderServiceExtensions
    {
        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientSessionContext clientContext,
            uint messageStreamId,
            uint chunkStreamId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null)
        {
            var properties = new Dictionary<string, object?>
            {
                { RtmpArgumentNames.Level, level },
                { RtmpArgumentNames.Code, code },
                { RtmpArgumentNames.Description, description }
            };

            sender.SendCommandMessage(clientContext, messageStreamId, chunkStreamId, "onStatus", 0, null, new List<object?> { properties }, amfEncodingType, callback);
        }

        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IReadOnlyList<IRtmpClientSessionContext> clientContexts,
            uint messageStreamId,
            uint chunkStreamId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var properties = new Dictionary<string, object?>
            {
                { RtmpArgumentNames.Level, level },
                { RtmpArgumentNames.Code, code },
                { RtmpArgumentNames.Description, description }
            };

            sender.SendCommandMessage(clientContexts, messageStreamId, chunkStreamId, "onStatus", 0, null, new List<object?> { properties }, amfEncodingType);
        }

        public static ValueTask SendOnStatusCommandMessageAsync(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientSessionContext clientContext,
            uint messageStreamId,
            uint chunkStreamId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new ValueTaskCompletionSource();
            sender.SendOnStatusCommandMessage(clientContext, messageStreamId, chunkStreamId, level, code, description, amfEncodingType, _ => tcs.SetResult());
            return tcs.Task;
        }
    }
}
