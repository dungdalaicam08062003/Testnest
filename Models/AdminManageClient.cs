using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public class AdminManageClient
    {
        public class ClientMainInfo()
        {
            public int ClientID { get; set; } 
            public string ClientName { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public string ClientAddress { get; set; } = "";
            public int TotalOrder { get; set; }  
        }
        public record AdminClientDetail()
        {
            
            public string ClientName { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public string ClientAddress { get; set; } = "";
            public int TotalOrder { get; set; }
            public List<AdminClientOrder?> adminClientDetails { get; set; } = [];
        }
        public record AdminClientOrder {
            public string orderCode { get; set; } = "";
            public DateTime createAt { get; set; } 
            public Decimal totalPrice { get; set; } 
            public string status { get; set; } = "";
        }
    }
}
