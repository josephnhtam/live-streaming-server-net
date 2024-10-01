using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred in the session loop")]
        public static partial void SessionLoopError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S0 Handled")]
        public static partial void HandshakeS0Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S1 Handled")]
        public static partial void HandshakeS1Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | Handshake S1 Handling Failed")]
        public static partial void HandshakeS1HandlingFailed(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S2 Handled")]
        public static partial void HandshakeS2Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | Handshake S2 Handling Failed")]
        public static partial void HandshakeS2HandlingFailed(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred while dispatching RTMP handshake complete event")]
        public static partial void DispatchingRtmpHandshakeCompleteEventError(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Trace, "SessionId: {SessionId} | Acknowledgement received")]
        public static partial void AcknowledgementReceived(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | SetChunkSize: {InChunkSize}")]
        public static partial void SetChunkSize(this ILogger logger, uint sessionId, uint inChunkSize);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | WindowAcknowledgementSize: {InWindowAcknowledgementSize}")]
        public static partial void WindowAcknowledgementSize(this ILogger logger, uint sessionId, uint inWindowAcknowledgementSize);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | Failed to handle RTMP message")]
        public static partial void FailedToHandleRtmpMessage(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred while handling command result")]
        public static partial void CommandResultHandlingError(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | StreamId: {StreamId} | Subscribe stream not yet created")]
        public static partial void SubscribeStreamNotYetCreated(this ILogger logger, uint sessionId, uint streamId);

        [LoggerMessage(LogLevel.Error, "StreamId: {StreamId} | An error occurred while updating stream metadata")]
        public static partial void StreamMetaDataUpdateError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "StreamId: {StreamId} | An error occurred while receiving video data")]
        public static partial void VideoDataReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "StreamId: {StreamId} | An error occurred while receiving audio data")]
        public static partial void AudioDataReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "StreamId: {StreamId} | An error occurred while receiving status")]
        public static partial void StatusReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "StreamId: {StreamId} | An error occurred while receiving user control event")]
        public static partial void UserControlEventReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while updating bandwidth limit")]
        public static partial void BandwidthLimitUpdateError(this ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | StreamId: {StreamId} | Invalid onStatus parameters")]
        public static partial void InvalidOnStatusParameters(this ILogger logger, uint sessionId, uint streamId);
    }
}