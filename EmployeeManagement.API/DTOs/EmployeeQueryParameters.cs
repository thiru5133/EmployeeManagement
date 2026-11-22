namespace EmployeeManagement.API.DTOs
{
    /// <summary>
    /// Day 9 — Query parameters for filtering, sorting, and pagination.
    /// </summary>
    public class EmployeeQueryParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        /// <summary>Page number (1-based).</summary>
        public int Page { get; set; } = 1;

        /// <summary>Page size (maximum 50, default 10).</summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }

        /// <summary>Filter by department name (case-insensitive substring or exact).</summary>
        public string? Department { get; set; }

        /// <summary>Search term to filter by employee name or email.</summary>
        public string? SearchTerm { get; set; }

        /// <summary>Sort column: name, email, department, salary, joiningdate.</summary>
        public string? SortBy { get; set; } = "name";

        /// <summary>Sort order: true for descending, false for ascending (default).</summary>
        public bool IsDescending { get; set; } = false;
    }
}
