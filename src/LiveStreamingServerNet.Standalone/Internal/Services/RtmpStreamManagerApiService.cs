using LiveStreamingServerNet.AdminPanelUI.Dtos;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Standalone.Exceptions;
using LiveStreamingServerNet.Standalone.Internal.Mappers;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class RtmpStreamManagerApiService : IRtmpStreamManagerApiService
    {
        private readonly IRtmpStreamInfoManager _streamInfoManager;

        public RtmpStreamManagerApiService(IRtmpStreamInfoManager streamInfoManager)
        {
            _streamInfoManager = streamInfoManager;
        }

        public Task<GetStreamsResponse> GetStreamsAsync(GetStreamsRequest request)
        {
            var (page, pageSize, filter) = request;

            var streams = _streamInfoManager.GetStreamInfos();

            if (!string.IsNullOrWhiteSpace(filter))
                streams = streams.Where(x => x.StreamPath.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

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
            var splitIndex = streamId.IndexOf('@');

            if (splitIndex == -1 || !uint.TryParse(streamId.Substring(0, splitIndex), out var clientId))
                throw new ApiException(StatusCodes.Status400BadRequest, "Invalid stream id format.");

            var streamPath = streamId.Substring(splitIndex + 1);
            var stream = _streamInfoManager.GetStreamInfo(streamPath);

            if (stream == null || stream.Publisher.ClientId != clientId)
                throw new ApiException(StatusCodes.Status404NotFound, $"Stream ({streamId}) not found.");

            await stream.Publisher.DisconnectAsync(cancellation);
        }
    }
}
