FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app
EXPOSE 1935

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["samples/LiveStreamingServerNet.BasicDemo/LiveStreamingServerNet.BasicDemo.csproj", "samples/LiveStreamingServerNet.BasicDemo/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.BasicDemo/LiveStreamingServerNet.BasicDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.BasicDemo"
RUN dotnet build "./LiveStreamingServerNet.BasicDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.BasicDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.BasicDemo.dll"]