namespace ConsoleApp1.Models
{
    public record ProductListItem
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public string Brand { get; init; } = "";
        public string? Thumbnail { get; init; }
        public int Stock { get; init; }
        public DateTime CreateAt { get; init; }
    }
    public record ProductSpec{
        public string CPU { get; init; } = "";
        public string RAM { get; init; } = "";
        public string Storage { get; init; } = "";
        public string Display { get; set; } = "";
        public string GPU { get; set; } = "";
    }
   
    public record Pagination(
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages
        );
    public record ProductListResponse(
        IEnumerable<ProductListItem> Data,
        Pagination Pagination,
        object AppliedFilters //{search, sort}

        );
    public record ErrorResponse(
        string Code,
        string Message,
        object? Detail = null
        );
}
