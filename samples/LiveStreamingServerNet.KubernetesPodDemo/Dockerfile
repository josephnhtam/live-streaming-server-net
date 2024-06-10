FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["samples/LiveStreamingServerNet.KubernetesPodDemo/LiveStreamingServerNet.KubernetesPodDemo.csproj", "samples/LiveStreamingServerNet.KubernetesPodDemo/"]
COPY ["src/LiveStreamingServerNet/LiveStreamingServerNet.csproj", "src/LiveStreamingServerNet/"]
COPY ["src/LiveStreamingServerNet.KubernetesPod/LiveStreamingServerNet.KubernetesPod.csproj", "src/LiveStreamingServerNet.KubernetesPod/"]
COPY ["src/LiveStreamingServerNet.KubernetesPod.Redis/LiveStreamingServerNet.KubernetesPod.Redis.csproj", "src/LiveStreamingServerNet.KubernetesPod.Redis/"]
COPY ["src/LiveStreamingServerNet.Rtmp/LiveStreamingServerNet.Rtmp.csproj", "src/LiveStreamingServerNet.Rtmp/"]
COPY ["src/LiveStreamingServerNet.Networking/LiveStreamingServerNet.Networking.csproj", "src/LiveStreamingServerNet.Networking/"]
COPY ["src/LiveStreamingServerNet.Utilities/LiveStreamingServerNet.Utilities.csproj", "src/LiveStreamingServerNet.Utilities/"]
RUN dotnet restore "./samples/LiveStreamingServerNet.KubernetesPodDemo/./LiveStreamingServerNet.KubernetesPodDemo.csproj"
COPY . .
WORKDIR "/src/samples/LiveStreamingServerNet.KubernetesPodDemo"
RUN dotnet build "./LiveStreamingServerNet.KubernetesPodDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.KubernetesPodDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.KubernetesPodDemo.dll"]