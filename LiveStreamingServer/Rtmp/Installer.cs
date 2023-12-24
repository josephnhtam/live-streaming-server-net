using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveStreamingServer.Rtmp
{
    public static class Installer
    {
        public static IHostBuilder UseRtmpServer(IHostBuilder builder)
        {
            return builder;
        }
    }
}
