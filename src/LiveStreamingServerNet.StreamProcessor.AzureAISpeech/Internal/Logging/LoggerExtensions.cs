using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stream started (Id={Id})")]
        public static partial void TranscriptionStreamStarted(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stream stopped (Id={Id})")]
        public static partial void TranscriptionStreamStopped(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error running transcription (Id={Id})")]
        public static partial void ErrorRunningTranscription(this ILogger logger, int id, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription started (Id={Id})")]
        public static partial void TranscriptionStarted(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcription stopped (Id={Id})")]
        public static partial void TranscriptionStopped(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error streaming audio to transcoder (Id={Id})")]
        public static partial void ErrorStreamingAudioToTranscoder(this ILogger logger, int id, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Audio streaming to transcoder stopped (Id={Id})")]
        public static partial void AudioStreamingStopped(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcoding canceled (Id={Id})")]
        public static partial void TranscodingCanceledLog(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcoding stopped (Id={Id})")]
        public static partial void TranscodingStoppedLog(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcription canceled (Id={Id})")]
        public static partial void TranscriptionCanceledLog(this ILogger logger, int id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcriber session started (Id={Id}, SessionId={SessionId})")]
        public static partial void TranscriberSessionStarted(this ILogger logger, int id, string sessionId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcriber session stopped (Id={Id}, SessionId={SessionId})")]
        public static partial void TranscriberSessionStopped(this ILogger logger, int id, string sessionId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Transcriber canceled (Id={Id}, ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails})")]
        public static partial void TranscriberCanceled(this ILogger logger, int id, string errorCode, string errorDetails);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcribing text (Id={Id}, ResultId={ResultId}, SpeakerId={SpeakerId}, Text={Text}, Timestamp={Timestamp}, Duration={Duration})")]
        public static partial void TranscribingText(this ILogger logger, int id, string resultId, string speakerId, string text, TimeSpan timestamp, TimeSpan duration);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Transcribed text (Id={Id}, ResultId={ResultId}, SpeakerId={SpeakerId}, Text={Text}, Timestamp={Timestamp}, Duration={Duration})")]
        public static partial void TranscribedText(this ILogger logger, int id, string resultId, string speakerId, string text, TimeSpan timestamp, TimeSpan duration);
    }
}