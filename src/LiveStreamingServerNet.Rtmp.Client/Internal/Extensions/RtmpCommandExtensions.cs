namespace LiveStreamingServerNet.Rtmp.Client.Internal.Extensions
{
    using InternalRtmpCommand = Services.Contracts.RtmpCommand;
    using InternalRtmpCommandResponse = Services.Contracts.RtmpCommandResponse;
    using RtmpCommand = Client.Contracts.RtmpCommand;
    using RtmpCommandResponse = Client.Contracts.RtmpCommandResponse;

    internal static class RtmpCommandExtensions
    {
        public static InternalRtmpCommand ToInternal(this RtmpCommand command, uint messageStreamId, uint chunkStreamId)
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

        public static RtmpCommandResponse ToExternal(this InternalRtmpCommandResponse response)
        {
            return new(
                response.TransactionId,
                response.CommandObject,
                response.Parameters
            );
        }
    }
}
