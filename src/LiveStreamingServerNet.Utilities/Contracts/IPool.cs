namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IPool<TObject> where TObject : class
    {
        TObject Obtain();
        void Recycle(TObject obj);
        int GetPooledCount();
    }
}