using LiveStreamingServerNet.AdminPanelUI.Dtos;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Standalone.Exceptions
{
    /// <summary>
    /// Exception that represents API errors with HTTP status codes and structured error responses.
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code for this error.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets the structured error response.
        /// </summary>
        public ErrorResponse ErrorResponse { get; }

        /// <summary>
        /// Creates an API exception with a status code and message.
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Error message</param>
        public ApiException(int statusCode, string message) : this(statusCode, new ErrorResponse(message)) { }

        /// <summary>
        /// Creates an API exception with a status code, message, and field-specific errors.
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Error message</param>
        /// <param name="errors">Dictionary of field-specific errors</param>
        public ApiException(int statusCode, string message, IReadOnlyDictionary<string, string> errors) :
            this(statusCode, new ErrorResponse(message, errors))
        { }

        /// <summary>
        /// Creates an API exception with a status code and error response.
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errorResponse">Structured error response</param>
        public ApiException(int statusCode, ErrorResponse errorResponse) : base(errorResponse.Message)
        {
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        /// <summary>
        /// Creates an API exception with default 500 Internal Server Error status.
        /// </summary>
        public ApiException() : this(StatusCodes.Status500InternalServerError, "Unknown error.") { }

        /// <summary>
        /// Creates an API exception with default 500 status and custom message.
        /// </summary>
        /// <param name="message">Error message</param>
        public ApiException(string? message) : this(StatusCodes.Status500InternalServerError, message ?? "Unknown error.") { }

        /// <summary>
        /// Creates an API exception with default 500 status, message and inner exception.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">The exception that caused this exception</param>
        public ApiException(string? message, Exception? innerException) :
            this(StatusCodes.Status500InternalServerError, message ?? innerException?.Message ?? "Unknown error.")
        { }
    }
}
