namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpClientController
    {
        void Connect(string appName);
        void Connect(string appName, IDictionary<string, object> information);
        void CreateStream();
    }

    public record struct CommandResult(bool Success, IDictionary<string, object> CommandObject, object? Parameters);
}
