namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    /// <summary>
    /// Defines an interface for objects that participate in object pooling.
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// Called when the object is obtained from the pool.
        /// Use this method to initialize or reset the object for use.
        /// </summary>
        void OnObtained();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// Use this method to clean up or reset the object for future reuse.
        /// </summary>
        void OnReturned();
    }
}