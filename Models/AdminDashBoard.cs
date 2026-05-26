using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public class AdminDashBoard
    {
        public record DashBoardInfomation
        {
            public int totalProduct { get; set; }
            public int totalOrder { get; set; }
            public int totalOrderIn24hour { get; set; }
            public int stockOfproductSmall { get; set; }
            public List<DashBoardOrder> listDashBoardOrders { get; set; } = [];

        }
        public record DashBoardOrder
        {
            public string orderCode { get; set; } = "";
            public string clientName { get; set; } = "";
            public string phoneNumber { get; set; } = "";
            public string status { get; set; } = "";
            public decimal totalPrice { get; set; }
            public DateTime createAt { get; set; }
        }
    }
}
