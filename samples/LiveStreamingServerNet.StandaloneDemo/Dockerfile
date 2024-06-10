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
COPY ["samples/LiveStreamingServerNet.StandaloneDemo/LiveStreamingServerNet.StandaloneDemo.csproj", "samples/LiveStreamingServerNet.StandaloneDemo/"]
COPY ["src/LiveStreamingServerNet.AdminPanelUI/LiveStreamingServerNet.AdminPanelUI.csproj", "src/LiveStreamingServerNet.AdminPanelUI/"]
COPY ["src/LiveStreamingServerNet.Flv/LiveStreamingServerNet.Flv.csproj", "src/LiveStreamingServerNet.Flv/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
COPY ["src/LiveStreamingServerNet.Standalone/LiveStreamingServerNet.Standalone.csproj", "src/LiveStreamingServerNet.Standalone/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.StandaloneDemo/LiveStreamingServerNet.StandaloneDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.StandaloneDemo"
RUN dotnet build "./LiveStreamingServerNet.StandaloneDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.StandaloneDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.StandaloneDemo.dll"]