using System;
using System.Collections.Generic;
using System.Text;

namespace Websitecomputer.Models
{
    public record Order
    {
        public int? OrderID { get; init; }
        public int ClientID { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? StatusOrders { get; set; } = "";
        public DateTime CreateAt { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public string Address { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
    }
    public record OrderRequest
    {
        public string ClientCode { get; set; } = "";
        public string ProductCode { get; set; } = " ";
        public int Quantity { get; set; }
        public string Address { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
    }
}
