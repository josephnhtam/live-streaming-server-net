using LiveStreamingServerNet.Common.Dtos;

namespace LiveStreamingServerNet.Standalone.Services.Contracts
{
    internal interface IRtmpStreamManagerApiService
    {
        Task<GetStreamsResponse> GetStreamsAsync(GetStreamsRequest request);
        Task DeleteStreamAsync(string streamId);
    }
}
