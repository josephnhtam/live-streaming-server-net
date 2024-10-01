﻿using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace LiveStreamingServerNet.RtmpClientPlayDemo
{
    public class Program
    {
        /// <summary>
        /// Connect to rtmp://127.0.0.1/live/demo and save the stream to output.flv.
        /// </summary>
        public static async Task Main()
        {
            var rtmpClient = RtmpClientBuilder.Create()
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Trace))
                .Build();

            var logger = rtmpClient.Services.GetRequiredService<ILogger<Program>>();

            var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1935);
            var information = new Dictionary<string, object> { ["tcUrl"] = "rtmp://127.0.0.1/live" };
            await rtmpClient.ConnectAsync(serverEndPoint, "live", information);

            var rtmpStream = await rtmpClient.CreateStreamAsync();

            await RecordStreamAsFlvAsync("demo", rtmpClient, rtmpStream, logger);
        }

        private static async Task RecordStreamAsFlvAsync(string streamName, IRtmpClient rtmpClient, IRtmpStream rtmpStream, ILogger<Program> logger)
        {
            using var fileStream = new FileStream("output.flv", FileMode.Create, FileAccess.Write, FileShare.Read);
            using var binaryWriter = new BinaryWriter(fileStream);

            using var flvWriter = new FlvWriter(binaryWriter);
            flvWriter.WriteHeader(true, true);

            rtmpStream.Subscribe.OnStreamMetaDataReceived += (sender, e) =>
                logger.LogInformation($"Stream meta data received: {JsonSerializer.Serialize(e.StreamMetaData)}");

            rtmpStream.Subscribe.OnVideoDataReceived += (sender, e) =>
                flvWriter.WriteTag(FlvTagType.Video, e.Timestamp, e.RentedBuffer.AsSpan());

            rtmpStream.Subscribe.OnAudioDataReceived += (sender, e) =>
                flvWriter.WriteTag(FlvTagType.Audio, e.Timestamp, e.RentedBuffer.AsSpan());

            rtmpStream.OnUserControlEventReceived += (sender, e) =>
                logger.LogInformation($"User control event received: {e.EventType}");

            rtmpStream.OnStatusReceived += (sender, e) =>
            {
                logger.LogInformation($"Status received: {e.Code}");

                if (e.Code == RtmpStreamStatusCodes.PlayUnpublishNotify)
                {
                    rtmpClient.Stop();
                }
            };

            rtmpStream.Subscribe.Play(streamName);
            await rtmpClient.UntilStoppedAsync();
        }
    }
}
