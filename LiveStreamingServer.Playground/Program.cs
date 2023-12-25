using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp;
using LiveStreamingServer.Rtmp.Core.Utilities;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServer.Playground
{

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var rtmpServer = new RtmpServer();
            var runTask = rtmpServer.RunAsync(new IPEndPoint(IPAddress.Any, 9999));

            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 9999);
            var networkStream = client.GetStream();

            // C0
            networkStream.WriteByte(0);
            await networkStream.FlushAsync();

            var netBuffer = new NetBuffer();

            while (true)
            {
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    continue;


                // Write the message payload
                netBuffer.Position = 4;
                netBuffer.Write(input);

                // Write the message header
                netBuffer.Position = 0;
                netBuffer.Write(netBuffer.Size - 4);

                netBuffer.Flush(networkStream);
                await networkStream.FlushAsync();
            }

            await runTask;
        }
    }
}
