namespace LiveStreamingServerNet.Rtmp.Client.Internal.Extensions
{
    internal static class RtmpCommandExtensions
    {
        public static RtmpCommandMessage ToMessage(this RtmpCommand command, uint messageStreamId, uint chunkStreamId)
        {
            return new(
                messageStreamId,
                chunkStreamId,
                command.CommandName,
                command.CommandObject,
                command.Parameters,
                command.AmfEncodingType
            );
        }
    }
}
