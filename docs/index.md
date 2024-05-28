# Live-Streaming-Server-Net

![build and test](https://github.com/josephnhtam/live-streaming-server-net/actions/workflows/build_and_test.yaml/badge.svg)
[![Nuget](https://img.shields.io/nuget/v/LiveStreamingServerNet)](https://www.nuget.org/packages/LiveStreamingServerNet/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Live-Streaming-Server-Net is a high-performance and flexible toolset which allows you to build your own live streaming server using .NET.

## Features

- **RTMP/RTMPS protocol**: Supports the RTMP and RTMPS protocols for streaming audio, video, and data.
- **HTTP-FLV/WebSocket-FLV with ASP.NET CORE**: Provides support for serving FLV live streams using HTTP-FLV and WebSocket-FLV protocols within an ASP.NET Core application.
- **Transmuxing RTMP streams into HLS streams**: Allows you to transmux RTMP streams into HLS (HTTP Live Streaming) streams using the built-in HLS transmuxer.
- **Integration with FFmpeg**: Provides support for processing the incoming RTMP stream with FFmpeg, for example, to create an MP4 archive.
- **GOP caching**: Supports caching the Group of Pictures (GOP) to ensure immediate availability of live streaming content.
- **Custom authorization**: Enables you to implement custom authorization mechanisms for accessing live streams.
- **Admin panel**: Includes an admin panel that provides an user interface for managing and monitoring the live streaming server.
- **Cloud Storage Integration**: Enabling real-time uploading of HLS files to cloud storage services like Azure Blob Storage, Google Cloud Storage, and AWS S3, which ensures scalable and efficient HLS stream distribution through CDN.

## In-Progress

- **Custom Kubernetes Operator and Kubernetes Integration**: The objective is to achieve automatic horizontal autoscaling by scaling out the pods when more streams are published, and scaling in the pods when streams are deleted, all without affecting the existing connections.
- **Redis Integration**: Integrate with Redis to share stream information among pods in the fleet.

## Roadmap

- **Edge Server**: Although edge servers are not necessary for serving HLS, they are required for serving RTMP and FLV streams in a cluster configuration.

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
      <th>LiveStreamingServerNet</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Standalone</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Standalone"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Standalone.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.AdminPanelUI</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.AdminPanelUI"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.AdminPanelUI.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Flv</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Flv"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Flv.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Networking</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Rtmp</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.AmazonS3</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AmazonS3"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AmazonS3.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.AzureBlobStorage</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Utilities</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Utilities"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Utilities?logo=nuget"></a></td>
    </tr>
  </tbody>
</table>
