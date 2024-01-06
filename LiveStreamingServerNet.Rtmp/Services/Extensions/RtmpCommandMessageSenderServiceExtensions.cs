using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Extensions
{
    internal static class RtmpCommandMessageSenderServiceExtensions
    {
        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientPeerContext peerContext,
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

            sender.SendCommandMessage(peerContext, publishStreamChunkId, "onStatus", 0, null, [properties], amfEncodingType, callback);
        }

        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IList<IRtmpClientPeerContext> peerContexts,
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

            sender.SendCommandMessage(peerContexts, publishStreamChunkId, "onStatus", 0, null, [properties], amfEncodingType);
        }

        public static Task SendOnStatusCommandMessageAsync(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientPeerContext peerContext,
            uint publishStreamChunkId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new TaskCompletionSource();
            sender.SendOnStatusCommandMessage(peerContext, publishStreamChunkId, level, code, description, amfEncodingType, tcs.SetResult);
            return tcs.Task;
        }
    }
}
