FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/LiveStreamingServerNet.Operator/LiveStreamingServerNet.Operator.csproj", "src/LiveStreamingServerNet.Operator/"]
RUN dotnet restore "./src/LiveStreamingServerNet.Operator/./LiveStreamingServerNet.Operator.csproj"
COPY . .
WORKDIR "/src/src/LiveStreamingServerNet.Operator"
RUN dotnet build "./LiveStreamingServerNet.Operator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LiveStreamingServerNet.Operator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveStreamingServerNet.Operator.dll"]