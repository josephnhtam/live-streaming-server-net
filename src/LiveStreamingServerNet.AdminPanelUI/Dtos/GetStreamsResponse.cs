namespace LiveStreamingServerNet.AdminPanelUI.Dtos
{
    public record GetStreamsResponse(IList<StreamDto> Streams, int TotalCount);
}
