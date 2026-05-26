using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public record Order
    {
        //public int? OrderCode { get; init; }
        public string OrderCode { get; init; } = "";
        public string ClientCode { get; set; } = "";
        public decimal? TotalPrice { get; set; }
        public string? StatusOrders { get; set; } = "";
        public DateTime CreateAt { get; set; }
        //public int ProductID { get; set; }
        //public int Quantity { get; set; }
        public string Address { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
    }
    public record OrderItem
    {
        public string OrderCode { get; set; } = "";
        public string ClientName { get; set; } = " ";
        public int quantity { get; set; }
        public string ProductName { get; set; } = "";
        public decimal price { get; set; }
        public decimal totalPrice { get; set; } 
    }
    public record OrderRequest
    {
        public string clientCode { get; set; } = "";
        public string productCode { get; set; } = " ";
        public int quantity { get; set; }
        public string address { get; set; } = "";
        public string phoneNumber { get; set; } = "";
    }
    public record UpdateOrder
    {
        public string orderCode { get; set; } = "";
        public string address { get; set; } = "";
        public string phoneNumber { get; set; } = "";
    }
    public record UpdateOrderStatus
    {
        public string orderCode { get; set; } = "";
        public string status { get; set; } = "";
    }
    public record CreateOrderRequest(
        string ClientCode, 
        string Address,
        string PhoneNumber,
        List<OrderItemRequest> orderItem
    );
    public record OrderItemRequest(
        string ProductCode,
        int Quantity
    );
    //public record CreateOrderRequest
    //{
    //    public string ClientCode { get; set; } = " ";
    //    public string Address { get; set; } = "";
    //    public string PhoneNumber { get; set; } = "";
    //    public List<OrderItemRequest> orderItem { get; set; } = new();
    //}
    //public record OrderItemRequest
    //{
    //    public string ProductCode { get; set; } = " ";
    //    public int Quantity { get; set; }
    //}
}
