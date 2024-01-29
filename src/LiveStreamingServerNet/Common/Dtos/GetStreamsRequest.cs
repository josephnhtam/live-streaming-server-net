namespace LiveStreamingServerNet.Common.Dtos
{
    public record GetStreamsRequest(int page, int pageSize, string? filter);
}
