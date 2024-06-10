FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 1935

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["samples/LiveStreamingServerNet.HlsDemo/LiveStreamingServerNet.HlsDemo.csproj", "samples/LiveStreamingServerNet.HlsDemo/"]
COPY ["src/LiveStreamingServerNet.StreamProcessor/LiveStreamingServerNet.StreamProcessor.csproj", "src/LiveStreamingServerNet.StreamProcessor/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.HlsDemo/LiveStreamingServerNet.HlsDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.HlsDemo"
RUN dotnet build "./LiveStreamingServerNet.HlsDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.HlsDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.HlsDemo.dll"]