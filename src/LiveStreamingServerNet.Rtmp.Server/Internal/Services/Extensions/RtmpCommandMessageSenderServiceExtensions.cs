using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Common;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions
{
    internal static class RtmpCommandMessageSenderServiceExtensions
    {
        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IRtmpStreamContext streamContext,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null)
        {
            var properties = new Dictionary<string, object?>
            {
                { RtmpArguments.Level, level },
                { RtmpArguments.Code, code },
                { RtmpArguments.Description, description }
            };

            sender.SendCommandMessage(
                streamContext.ClientContext,
                streamContext.StreamId,
                streamContext.CommandChunkStreamId,
                "onStatus",
                0,
                null,
                new List<object?> { properties },
                amfEncodingType,
                callback
            );
        }

        public static void SendOnStatusCommandMessage(
            this IRtmpCommandMessageSenderService sender,
            IReadOnlyList<IRtmpStreamContext> streamContexts,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var properties = new Dictionary<string, object?>
            {
                { RtmpArguments.Level, level },
                { RtmpArguments.Code, code },
                { RtmpArguments.Description, description }
            };

            foreach (var batch in streamContexts.GroupBy(x => (x.StreamId, x.CommandChunkStreamId)))
            {
                var streamId = batch.Key.StreamId;
                var chunkStreamId = batch.Key.CommandChunkStreamId;
                var clientContexts = batch.Select(x => x.ClientContext).ToList();

                sender.SendCommandMessage(
                    clientContexts,
                    streamId,
                    chunkStreamId,
                    "onStatus",
                    0,
                    null,
                    new List<object?> { properties },
                    amfEncodingType
                );
            }
        }

        public static ValueTask SendOnStatusCommandMessageAsync(
            this IRtmpCommandMessageSenderService sender,
            IRtmpStreamContext streamContext,
            string level,
            string code,
            string description,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new ValueTaskCompletionSource();

            sender.SendOnStatusCommandMessage(
                streamContext,
                level,
                code,
                description,
                amfEncodingType,
                _ => tcs.SetResult()
            );

            return tcs.Task;
        }
    }
}
