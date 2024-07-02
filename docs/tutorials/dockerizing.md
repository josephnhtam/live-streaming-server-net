# Dockerizing the Application

## Runtime Image

Some of the packages have dependencies on the ASP.NET Core framework, including:

- LiveStreamingServerNet.Standalone
- LiveStreamingServerNet.AdminPanelUI
- LiveStreamingServerNet.Flv
- LiveStreamingServerNet.StreamProcessor.AspNetCore

Therefore when these packages are included in your application, or if your application depends on ASP.NET Core, please ensure that you use `mcr.microsoft.com/dotnet/aspnet:8.0` as the runtime image. Otherwise, you may choose to use `mcr.microsoft.com/dotnet/runtime:8.0` instead.

## FFmpeg Dependency

If your application requires FFmpeg, you can add the following command to install FFmpeg in the runtime image:

```
RUN apt-get update && apt-get install -y ffmpeg
```

The FFmpeg binary can then be found with `ExecutableFinder.FindExecutableFromPATH("ffmpeg")`

## Example Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y ffmpeg
EXPOSE 8080
EXPOSE 1935

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY your-project.csproj .
RUN dotnet restore "./your-project.csproj"
COPY . .
RUN dotnet build "./your-project.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./your-project.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "your-project.dll"]
```
