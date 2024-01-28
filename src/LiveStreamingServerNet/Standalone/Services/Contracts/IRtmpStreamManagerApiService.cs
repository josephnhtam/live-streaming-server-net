using LiveStreamingServerNet.Standalone.Dtos;

namespace LiveStreamingServerNet.Standalone.Services.Contracts
{
    internal interface IRtmpStreamManagerApiService
    {
        GetStreamsResponse GetStreams(GetStreamsRequest request);
    }

    public record GetStreamsRequest(int page, int pageSize, string? filter);
}
