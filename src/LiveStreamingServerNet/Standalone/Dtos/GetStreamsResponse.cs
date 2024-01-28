namespace LiveStreamingServerNet.Standalone.Dtos
{
    public record GetStreamsResponse(IList<StreamDto> Streams, int TotalCount);
}
