using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stream {Id} started.")]
        public static partial void TranscriptionStreamStarted(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stream {Id} stopped.")]
        public static partial void TranscriptionStreamStopped(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error running transcription")]
        public static partial void ErrorRunningTranscription(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription started.")]
        public static partial void TranscriptionStarted(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stopped.")]
        public static partial void TranscriptionStopped(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error streaming audio to transcoder")]
        public static partial void ErrorStreamingAudioToTranscoder(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Audio streaming to transcoder stopped.")]
        public static partial void AudioStreamingStopped(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcoding canceled")]
        public static partial void TranscodingCanceledLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcoding stopped")]
        public static partial void TranscodingStoppedLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcription canceled")]
        public static partial void TranscriptionCanceledLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcriber session started (ID: {SessionId})")]
        public static partial void TranscriberSessionStarted(this ILogger logger, string sessionId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcriber session stopped (ID: {SessionId})")]
        public static partial void TranscriberSessionStopped(this ILogger logger, string sessionId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Transcriber canceled (ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails})")]
        public static partial void TranscriberCanceled(this ILogger logger, string errorCode, string errorDetails);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcribing text: '{Text}' (ResultId: {ResultId}, SpeakerId: {SpeakerId}, Timestamp: {Timestamp}, Duration: {Duration})")]
        public static partial void TranscribingText(this ILogger logger, string resultId, string speakerId, string text, TimeSpan timestamp, TimeSpan duration);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcribed text: '{Text}' (ResultId: {ResultId}, SpeakerId: {SpeakerId}, Timestamp: {Timestamp}, Duration: {Duration})")]
        public static partial void TranscribedText(this ILogger logger, string resultId, string speakerId, string text, TimeSpan timestamp, TimeSpan duration);
    }
}