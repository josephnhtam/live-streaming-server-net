FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
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
COPY ["samples/LiveStreamingServerNet.HlsAuthenticationDemo/LiveStreamingServerNet.HlsAuthenticationDemo.csproj", "samples/LiveStreamingServerNet.HlsAuthenticationDemo/"]
COPY ["src/LiveStreamingServerNet.StreamProcessor/LiveStreamingServerNet.StreamProcessor.csproj", "src/LiveStreamingServerNet.StreamProcessor/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.HlsAuthenticationDemo/LiveStreamingServerNet.HlsAuthenticationDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.HlsAuthenticationDemo"
RUN dotnet build "./LiveStreamingServerNet.HlsAuthenticationDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.HlsAuthenticationDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.HlsAuthenticationDemo.dll"]