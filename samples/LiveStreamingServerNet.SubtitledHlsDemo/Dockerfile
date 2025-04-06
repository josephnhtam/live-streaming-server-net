FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y ffmpeg
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 1935

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["nuget.config", "."]
COPY ["samples/LiveStreamingServerNet.SubtitledHlsDemo/LiveStreamingServerNet.SubtitledHlsDemo.csproj", "samples/LiveStreamingServerNet.SubtitledHlsDemo/"]
COPY ["src/LiveStreamingServerNet.StreamProcessor.AspNetCore/LiveStreamingServerNet.StreamProcessor.AspNetCore.csproj", "src/LiveStreamingServerNet.StreamProcessor.AspNetCore/"]
COPY ["src/LiveStreamingServerNet.StreamProcessor/LiveStreamingServerNet.StreamProcessor.csproj", "src/LiveStreamingServerNet.StreamProcessor/"]
COPY ["src/LiveStreamingServerNet.Rtmp.Server/LiveStreamingServerNet.Rtmp.Server.csproj", "src/LiveStreamingServerNet.Rtmp.Server/"]
COPY ["src/LiveStreamingServerNet.Networking.Server/LiveStreamingServerNet.Networking.Server.csproj", "src/LiveStreamingServerNet.Networking.Server/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.StreamProcessor.AzureAISpeech/LiveStreamingServerNet.StreamProcessor.AzureAISpeech.csproj", "src/LiveStreamingServerNet.StreamProcessor.AzureAISpeech/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.SubtitledHlsDemo/LiveStreamingServerNet.SubtitledHlsDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.SubtitledHlsDemo"
RUN dotnet build "./LiveStreamingServerNet.SubtitledHlsDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.SubtitledHlsDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.SubtitledHlsDemo.dll"]