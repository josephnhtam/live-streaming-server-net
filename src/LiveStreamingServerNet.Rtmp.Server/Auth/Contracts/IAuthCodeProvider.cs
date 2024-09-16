namespace LiveStreamingServerNet.Rtmp.Server.Auth.Contracts
{
    public interface IAuthCodeProvider
    {
        string GetAuthCode();
    }
}
