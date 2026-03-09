using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Websitecomputer.Page
{
     public record CartHeader()
    {
        public int cartID { get; init; }
    }
    public record CartItem()
    {
        public int cartItemID { get; init; }
        public int cartID { get; init; }
        public int productID { get; init; }
        public int quantity { get; init; }
        public string productName { get; init; } = "";
        public string thumbnail { get; set; } = "";
        public decimal price { get; init; }
    }
    
}
