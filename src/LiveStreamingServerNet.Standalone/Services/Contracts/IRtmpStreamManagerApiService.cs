using LiveStreamingServerNet.AdminPanelUI.Dtos;

namespace LiveStreamingServerNet.Standalone.Services.Contracts
{
    /// <summary>
    /// Provides API operations for managing RTMP streams.
    /// </summary>
    public interface IRtmpStreamManagerApiService
    {
        /// <summary>
        /// Gets information about active RTMP streams.
        /// </summary>
        /// <param name="request">The request parameters for filtering streams</param>
        /// <returns>Information about matching RTMP streams</returns>
        Task<GetStreamsResponse> GetStreamsAsync(GetStreamsRequest request);

        /// <summary>
        /// Deletes an RTMP stream.
        /// </summary>
        /// <param name="streamId">The ID of the stream to delete</param>
        /// <param name="cancellation">Optional cancellation token</param>
        Task DeleteStreamAsync(string streamId, CancellationToken cancellation = default);
    }
}
