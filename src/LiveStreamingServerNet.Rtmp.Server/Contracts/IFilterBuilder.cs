namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    /// <summary>
    /// Builds inclusion/exclusion filters.
    /// </summary>
    /// <typeparam name="TItem">The type of item to filter</typeparam>
    public interface IFilterBuilder<TItem> where TItem : struct
    {
        /// <summary>
        /// Adds an item to the inclusion list.
        /// </summary>
        /// <param name="item">The item to include</param>
        /// <returns>The builder instance for method chaining</returns>
        IFilterBuilder<TItem> Include(TItem item);

        /// <summary>
        /// Adds an item to the exclusion list.
        /// </summary>
        /// <param name="item">The item to exclude</param>
        /// <returns>The builder instance for method chaining</returns>
        IFilterBuilder<TItem> Exclude(TItem item);
    }
}