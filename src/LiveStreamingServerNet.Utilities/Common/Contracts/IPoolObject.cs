namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    public interface IPoolObject
    {
        void OnObtained();
        void OnReturned();
    }
}
