using System.Reflection;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class RemuxingConfiguration
    {
        public required string InputBasePath { get; set; }
        public required string OutputDirectoryPath { get; set; }

        public RemuxingConfiguration()
        {
            InputBasePath = "rtmp://localhost:1935";
            OutputDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "TransmuxerOutput");
        }
    }
}
