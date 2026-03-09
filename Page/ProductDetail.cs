using System;
using ConsoleApp1.Models;
using System.Collections.Generic;
using System.Text;

namespace Websitecomputer.Page
{
    public record ProductMainInfo
    {
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string Brand { get; init; } = "";
        public string? Thumbnail { get; init; }
    }

    public record ProductDetail
    {
        public string ProductId { get; init; } = "";
        public string Name { get; init; } = "";
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string Brand { get; init; } = "";
        public string? Thumbnail { get; init; }
        public List<string> Images { get; init; } = new();
        public ProductSpec? Specs { get; init; }
    }
}
