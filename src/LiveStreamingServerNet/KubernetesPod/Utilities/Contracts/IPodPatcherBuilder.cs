namespace LiveStreamingServerNet.KubernetesPod.Utilities.Contracts
{
    public interface IPodPatcherBuilder
    {
        IPodPatcherBuilder SetLabel(string key, string value);
        IPodPatcherBuilder RemoveLabel(string key);
        IPodPatcherBuilder SetAnnotation(string key, string value);
        IPodPatcherBuilder RemoveAnnotation(string key);
    }
}
