using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred in the session loop (SessionId={SessionId})")]
        public static partial void SessionLoopError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "Handshake S0 Handled (SessionId={SessionId})")]
        public static partial void HandshakeS0Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "Handshake S1 Handled (SessionId={SessionId})")]
        public static partial void HandshakeS1Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "Handshake S1 Handling Failed (SessionId={SessionId})")]
        public static partial void HandshakeS1HandlingFailed(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "Handshake S2 Handled (SessionId={SessionId})")]
        public static partial void HandshakeS2Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "Handshake S2 Handling Failed (SessionId={SessionId})")]
        public static partial void HandshakeS2HandlingFailed(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP handshake complete event (SessionId={SessionId})")]
        public static partial void DispatchingRtmpHandshakeCompleteEventError(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Trace, "Acknowledgement received (SessionId={SessionId})")]
        public static partial void AcknowledgementReceived(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Warning, "Maximum in-chunk size exceeded (SessionId={SessionId}, InChunkSize={InChunkSize}, MaxInChunkSize={MaxInChunkSize})")]
        public static partial void MaxInChunkSizeExceeded(this ILogger logger, uint sessionId, uint inChunkSize, uint maxInChunkSize);

        [LoggerMessage(LogLevel.Debug, "SetChunkSize (SessionId={SessionId}, InChunkSize={InChunkSize})")]
        public static partial void SetChunkSize(this ILogger logger, uint sessionId, uint inChunkSize);

        [LoggerMessage(LogLevel.Debug, "WindowAcknowledgementSize (SessionId={SessionId}, InWindowAcknowledgementSize={InWindowAcknowledgementSize})")]
        public static partial void WindowAcknowledgementSize(this ILogger logger, uint sessionId, uint inWindowAcknowledgementSize);

        [LoggerMessage(LogLevel.Error, "Failed to handle RTMP message (SessionId={SessionId})")]
        public static partial void FailedToHandleRtmpMessage(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while handling command result (SessionId={SessionId})")]
        public static partial void CommandResultHandlingError(this ILogger logger, uint sessionId, Exception ex);

        [LoggerMessage(LogLevel.Error, "Subscribe stream not yet created (SessionId={SessionId}, StreamId={StreamId})")]
        public static partial void SubscribeStreamNotYetCreated(this ILogger logger, uint sessionId, uint streamId);

        [LoggerMessage(LogLevel.Error, "An error occurred while updating stream metadata (StreamId={StreamId})")]
        public static partial void StreamMetaDataUpdateError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while receiving video data (StreamId={StreamId})")]
        public static partial void VideoDataReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while receiving audio data (StreamId={StreamId})")]
        public static partial void AudioDataReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while receiving status (StreamId={StreamId})")]
        public static partial void StatusReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while receiving user control event (StreamId={StreamId})")]
        public static partial void UserControlEventReceiveError(this ILogger logger, uint streamId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while updating bandwidth limit")]
        public static partial void BandwidthLimitUpdateError(this ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Error, "Invalid onStatus parameters (SessionId={SessionId}, StreamId={StreamId})")]
        public static partial void InvalidOnStatusParameters(this ILogger logger, uint sessionId, uint streamId);
    }
}