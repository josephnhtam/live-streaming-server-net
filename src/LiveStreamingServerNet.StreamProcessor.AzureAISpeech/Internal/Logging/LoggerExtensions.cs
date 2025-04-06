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

        [LoggerMessage(Level = LogLevel.Information, Message = "Speech recognition started.")]
        public static partial void SpeechRecognitionStarted(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Speech recognition stopped.")]
        public static partial void SpeechRecognitionStopped(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Error streaming audio to transcoder")]
        public static partial void ErrorStreamingAudioToTranscoder(this ILogger logger, Exception ex);

        [LoggerMessage(Level = LogLevel.Information, Message = "Audio streaming to transcoder stopped.")]
        public static partial void AudioStreamingStopped(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Transcoding canceled")]
        public static partial void TranscodingCanceledLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Transcoding stopped")]
        public static partial void TranscodingStoppedLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Transcription canceled")]
        public static partial void TranscriptionCanceledLog(this ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Recognizer session started (ID: {SessionId})")]
        public static partial void RecognizerSessionStarted(this ILogger logger, string sessionId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Recognizer session stopped (ID: {SessionId})")]
        public static partial void RecognizerSessionStopped(this ILogger logger, string sessionId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Recognizer canceled: ErrorCode={ErrorCode}, ErrorDetails={ErrorDetails}")]
        public static partial void RecognizerCanceled(this ILogger logger, string errorCode, string errorDetails);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Recognizing text: '{Text}', Timestamp: {Timestamp}, Duration: {Duration}")]
        public static partial void RecognizingText(this ILogger logger, string text, TimeSpan timestamp, TimeSpan duration);
    }
}