namespace WebsiteComputer.Models
{
    public record ProductItem
    {
        public string id { get; init; } = "";
        public string name { get; init; } = "";
        public decimal price { get; init; }
        public decimal priceAfterDiscount { get; set; }
        public string brand { get; init; } = "";
        public string category { get; set; } = "";
        public string? thumbnail { get; init; }
        public int stock { get; init; }
        public DateTime createAt { get; init; }
        public int? voucherId { get; init; } = null;
    }

    public record CreateProductRequest
    {
        public CreateUpdateProduct ProductInfo { get; init; } = default!;
        public List<ProductSpec?> ProductSpecs { get; init; } = [];
    }
    public record Pagination(
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages
        );
    public record ProductListResponse(
        IEnumerable<ProductItem> Data,
        Pagination Pagination,
        object AppliedFilters //{search, sort}

        );
    public record ErrorResponse(
        string Code,
        string Message,
        object? Detail = null
        );
}
