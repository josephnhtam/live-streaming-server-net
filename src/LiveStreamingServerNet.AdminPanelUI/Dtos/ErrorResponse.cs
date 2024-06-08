namespace LiveStreamingServerNet.AdminPanelUI.Dtos
{
    public class ErrorResponse
    {
        public string Message { get; }
        public IReadOnlyDictionary<string, string> Errors { get; }

        private static IReadOnlyDictionary<string, string> _emptyErrors = new Dictionary<string, string>();

        public ErrorResponse(string message)
        {
            Message = message;
            Errors = _emptyErrors;
        }

        public ErrorResponse(string message, IReadOnlyDictionary<string, string> errors)
        {
            Message = message;
            Errors = errors;
        }
    }
}
