using LiveStreamingServerNet.AdminPanelUI.Dtos;
using LiveStreamingServerNet.Standalone.Exceptions;
using LiveStreamingServerNet.Standalone.Internal.Mappers;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerApiService : IRtmpStreamManagerApiService
    {
        private readonly IRtmpStreamManagerService _streamManagerService;

        public RtmpStreamManagerApiService(IRtmpStreamManagerService streamManagerService)
        {
            _streamManagerService = streamManagerService;
        }

        public Task<GetStreamsResponse> GetStreamsAsync(GetStreamsRequest request)
        {
            var (page, pageSize, filter) = request;

            var streams = _streamManagerService.GetStreams().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter))
                streams = streams.Where(x => x.StreamPath.Contains(filter, StringComparison.OrdinalIgnoreCase));

            var totalCount = streams.Count();

            var result = streams
                .OrderByDescending(x => x.StartTime)
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => s.ToDto())
                .ToList();

            return Task.FromResult(new GetStreamsResponse(result, totalCount));
        }

        public async Task DeleteStreamAsync(string streamId, CancellationToken cancellation)
        {
            var stream = _streamManagerService.GetStream(streamId);

            if (stream == null)
                throw new ApiException(StatusCodes.Status404NotFound, $"Stream ({streamId}) not found.");

            await stream.Client.DisconnectAsync(cancellation);
        }
    }
}
