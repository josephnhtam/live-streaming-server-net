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
COPY ["samples/LiveStreamingServerNet.HlsWithFFmpegDemo/LiveStreamingServerNet.HlsWithFFmpegDemo.csproj", "samples/LiveStreamingServerNet.HlsWithFFmpegDemo/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.HlsWithFFmpegDemo/LiveStreamingServerNet.HlsWithFFmpegDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.HlsWithFFmpegDemo"
RUN dotnet build "./LiveStreamingServerNet.HlsWithFFmpegDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.HlsWithFFmpegDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.HlsWithFFmpegDemo.dll"]