using System.Text;

namespace LiveStreamingServerNet.WebRTC.Ice
{
    public record IceCredentials(string UFragLocal, string PwdLocal, string UFragRemote, string PwdRemote)
    {
        public byte[] PwdLocalBytes { get; } = Encoding.UTF8.GetBytes(PwdLocal);
        public byte[] PwdRemoteBytes { get; } = Encoding.UTF8.GetBytes(PwdRemote);
    }
}
