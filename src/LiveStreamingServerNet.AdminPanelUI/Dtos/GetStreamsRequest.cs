namespace LiveStreamingServerNet.AdminPanelUI.Dtos
{
    public record GetStreamsRequest(int page, int pageSize, string? filter);
}
