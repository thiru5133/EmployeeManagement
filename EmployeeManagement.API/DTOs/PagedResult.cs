namespace EmployeeManagement.API.DTOs
{
    /// <summary>
    /// Day 9 — Generic paginated response wrapper.
    ///
    /// Wraps any collection with page metadata so clients know
    /// how to navigate results without loading everything at once.
    ///
    /// Example response:
    /// {
    ///   "data": [...],
    ///   "page": 1,
    ///   "pageSize": 10,
    ///   "totalCount": 42,
    ///   "totalPages": 5,
    ///   "hasPrevious": false,
    ///   "hasNext": true
    /// }
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>The items on the current page.</summary>
        public IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();

        /// <summary>Current page number (1-based).</summary>
        public int Page { get; init; }

        /// <summary>Number of items per page.</summary>
        public int PageSize { get; init; }

        /// <summary>Total number of items across all pages.</summary>
        public int TotalCount { get; init; }

        /// <summary>Total number of pages.</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasPrevious => Page > 1;
        public bool HasNext     => Page < TotalPages;
    }
}
