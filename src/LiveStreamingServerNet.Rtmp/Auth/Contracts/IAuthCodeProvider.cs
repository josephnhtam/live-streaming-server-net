namespace LiveStreamingServerNet.Rtmp.Auth.Contracts
{
    public interface IAuthCodeProvider
    {
        string GetAuthCode();
    }
}
