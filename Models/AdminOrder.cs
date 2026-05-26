using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public class AdminOrder
    {
        public record GetOrderList
        {
            public string orderID { get; set; } = "";
            public string clientName { get; set; } = "";
            public DateTime creatAt { get; set; } 
            public string phoneNumber { get; set; } = "";
            public string Address { get; set; } = "";
            public decimal totalMoney { get; set; }
            public decimal totalMoneyAfterDiscount { get; set; }

        }
        public record OrderDetail
        {
            public GetOrderList order { get; set; }
            public List<OrderItems> listOrderItem { get; set; } = [];
        }
        public record OrderItems
        {
            public string productName { get; set; } = "";
            public int quantity { get; set; }
            public decimal price { get; set; }
            public decimal totalPrice { get; set; }
            public decimal totalPriceAfterDiscount { get; set; } = 0;

        }
    }
}
