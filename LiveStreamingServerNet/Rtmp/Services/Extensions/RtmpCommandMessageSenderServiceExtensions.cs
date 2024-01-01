using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Extensions
{
    public static class RtmpCommandMessageSenderServiceExtensions
    {
        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientPeerContext peerContext,
            uint publishStreamId,
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

            sender.SendCommandMessage(peerContext, publishStreamId, "onStatus", 0, null, [properties], amfEncodingType, callback);
        }

        public static Task SendOnStatusCommandMessageAsync(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientPeerContext peerContext,
            uint publishStreamId,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new TaskCompletionSource();
            sender.SendOnStatusCommandMessage(peerContext, publishStreamId, level, code, description, amfEncodingType, tcs.SetResult);
            return tcs.Task;
        }

        public static void SendSeekCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpClientPeerContext peerContext,
            uint publishStreamId,
            double milliSeconds,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action? callback = null)
        {
            sender.SendCommandMessage(peerContext, publishStreamId, "seek", 0, null, [milliSeconds], amfEncodingType, callback);
        }
    }
}
