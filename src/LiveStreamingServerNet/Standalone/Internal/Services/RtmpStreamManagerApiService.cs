using LiveStreamingServerNet.Standalone.Dtos;
using LiveStreamingServerNet.Standalone.Dtos.Mappers;
using LiveStreamingServerNet.Standalone.Internal.Services.Contracts;
using LiveStreamingServerNet.Standalone.Services.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerApiService : IRtmpStreamManagerApiService
    {
        private readonly IRtmpStreamManagerService _streamManagerService;

        public RtmpStreamManagerApiService(IRtmpStreamManagerService streamManagerService)
        {
            _streamManagerService = streamManagerService;
        }

        public GetStreamsResponse GetStreams(GetStreamsRequest request)
        {
            var (page, pageSize, filter) = request;

            var streams = _streamManagerService.GetStreams()
                .OrderByDescending(x => x.StartTime)
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize);

            if (!string.IsNullOrWhiteSpace(filter))
                streams = streams.Where(x => x.StreamPath.Contains(filter, StringComparison.OrdinalIgnoreCase));

            return new GetStreamsResponse(streams.Select(s => s.ToDto()).ToList(), streams.Count());
        }
    }
}
