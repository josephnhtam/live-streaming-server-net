using LiveStreamingServerNet.Common.Dtos;
using LiveStreamingServerNet.Common.Exceptions;
using LiveStreamingServerNet.Standalone.Dtos.Mappers;
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

            var streams = _streamManagerService.GetStreams()
                .OrderByDescending(x => x.StartTime)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter))
                streams = streams.Where(x => x.StreamPath.Contains(filter, StringComparison.OrdinalIgnoreCase));

            var totalCount = streams.Count();

            var result = streams
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => s.ToDto())
                .ToList();

            return Task.FromResult(new GetStreamsResponse(result, totalCount));
        }

        public Task DeleteStreamAsync(string streamId)
        {
            var stream = _streamManagerService.GetStream(streamId);

            if (stream == null)
                throw new ApiException(StatusCodes.Status404NotFound, $"Stream ({streamId}) not found.");

            stream.Client.Disconnect();

            return Task.CompletedTask;
        }
    }
}
