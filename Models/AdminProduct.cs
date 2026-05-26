using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models
{
    public class AdminProduct
    {
        public record ProductGetList
        {
            public string productCode { get; set; } = " ";
            public string productName { get; set; } = " ";
            public decimal price { get; set; } 
            public int stock { get; set; }
        }
    }
}
