using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Extensions
{
    internal static class RtmpCommandMessageSenderServiceExtensions
    {
        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientContext clientContext,
            uint publishStreamChunkId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action? callback = null)
        {
            var properties = new Dictionary<string, object?>
            {
                { RtmpArgumentNames.Level, level },
                { RtmpArgumentNames.Code, code },
                { RtmpArgumentNames.Description, description }
            };

            sender.SendCommandMessage(clientContext, publishStreamChunkId, "onStatus", 0, null, new List<object?> { properties }, amfEncodingType, callback);
        }

        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IReadOnlyList<IRtmpClientContext> clientContexts,
            uint publishStreamChunkId,
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

            sender.SendCommandMessage(clientContexts, publishStreamChunkId, "onStatus", 0, null, new List<object?> { properties }, amfEncodingType);
        }

        public static Task SendOnStatusCommandMessageAsync(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientContext clientContext,
            uint publishStreamChunkId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new TaskCompletionSource();
            sender.SendOnStatusCommandMessage(clientContext, publishStreamChunkId, level, code, description, amfEncodingType, tcs.SetResult);
            return tcs.Task;
        }
    }
}
