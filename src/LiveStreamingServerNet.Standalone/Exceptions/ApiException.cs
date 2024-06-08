using LiveStreamingServerNet.AdminPanelUI.Dtos;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Standalone.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public ErrorResponse ErrorResponse { get; }

        public ApiException(int statusCode, string message) : this(statusCode, new ErrorResponse(message)) { }

        public ApiException(int statusCode, string message, IReadOnlyDictionary<string, string> errors) :
            this(statusCode, new ErrorResponse(message, errors))
        { }

        public ApiException(int statusCode, ErrorResponse errorResponse) : base(errorResponse.Message)
        {
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        public ApiException() : this(StatusCodes.Status500InternalServerError, "Unknown error.") { }

        public ApiException(string? message) : this(StatusCodes.Status500InternalServerError, message ?? "Unknown error.") { }

        public ApiException(string? message, Exception? innerException) :
            this(StatusCodes.Status500InternalServerError, message ?? innerException?.Message ?? "Unknown error.")
        { }
    }
}
