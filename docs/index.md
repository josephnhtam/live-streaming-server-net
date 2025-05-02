---
title: Overview

hide:
  - navigation
---

# Live-Streaming-Server-Net

![build and test](https://github.com/josephnhtam/live-streaming-server-net/actions/workflows/build_and_test.yaml/badge.svg)
[![Nuget](https://img.shields.io/nuget/v/LiveStreamingServerNet)](https://www.nuget.org/packages/LiveStreamingServerNet/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![JetBrains OSS](https://img.shields.io/badge/JetBrains-OSS-yellow.svg)](https://www.jetbrains.com/community/opensource)

Live-Streaming-Server-Net is a high-performance and flexible toolset which allows you to build your own live streaming server using .NET.

## Features

- **RTMP/RTMPS protocol**: Supports the RTMP and RTMPS protocols for streaming audio, video, and data.
- **RTMP Relay**: Supports relaying RTMP streams between servers, allowing building a scalable RTMP server cluster.
- **RTMP Client**: Provides a client library for connecting to RTMP servers and publishing/subscribing live streams.
- **HTTP-FLV/WebSocket-FLV with ASP.NET CORE**: Provides support for serving FLV live streams using HTTP-FLV and WebSocket-FLV protocols within an ASP.NET Core application.
- **Transmuxing RTMP streams into HLS streams**: Allows you to transmux RTMP streams into HLS (HTTP Live Streaming) streams using the built-in HLS transmuxer.
- **Transcoding RTMP streams into Adaptive HLS streams**: Integrates with FFmpeg to transcode RTMP streams into multiple-bitrate Adaptive HLS streams.
- **Integration with FFmpeg**: Provides support for processing the incoming RTMP stream with FFmpeg, for example, to create an MP4 archive.
- **GOP caching**: Supports caching the Group of Pictures (GOP) to ensure immediate availability of live streaming content.
- **Custom authorization**: Enables you to implement custom authorization mechanisms for accessing live streams.
- **Admin panel**: Includes an admin panel that provides an user interface for managing and monitoring the live streaming server.
- **Cloud Storage Integration**: Enabling real-time uploading of HLS files to cloud storage services like Azure Blob Storage, Google Cloud Storage, and AWS S3, which ensures scalable and efficient HLS stream distribution through CDN.
- **Realtime HLS Subtitle Transcription**: Integrates with Azure AI Speech to provide real-time transcription of HLS streams, automatically generating WebVTT subtitle files.
- **Codecs**: Supports AVC/H.264, HEVC/H.265, AAC, and MP3 codecs.

## NuGet Packages

<table>
  <thead>
    <tr>
      <th>Package</th>
      <th>Latest Version</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><b>LiveStreamingServerNet</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Standalone</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Standalone"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Standalone.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.AdminPanelUI</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.AdminPanelUI"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.AdminPanelUI.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Flv</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Flv"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Flv.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Networking</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Networking.Client</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking.Client"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.Client.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Networking.Server</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking.Server"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.Server.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Rtmp</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Rtmp.Client</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp.Client"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.Client.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Rtmp.Relay</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp.Relay"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.Relay.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Rtmp.Server</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp.Server"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.Server.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor.AmazonS3</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AmazonS3"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AmazonS3.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor.AspNetCore</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AspNetCore"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AspNetCore.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor.AzureAISpeech</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AzureAISpeech"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AzureAISpeech.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor.AzureBlobStorage</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <td><b>LiveStreamingServerNet.Utilities</b></td>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Utilities"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Utilities?logo=nuget"></a></td>
    </tr>
  </tbody>
</table>

## License

This project is licensed under the terms of the [MIT license](https://github.com/josephnhtam/live-streaming-server-net/blob/master/LICENSE).

## Acknowledgments

Special thanks to [JetBrains](https://www.jetbrains.com/) for providing the open-source software license that supports the development of this project.
