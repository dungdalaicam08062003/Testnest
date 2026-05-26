using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models.Policy
{
    public class Guarantee
    {
        public record GuaranteeProduct()
        {
            public string guaranteeID { get; set; } = "";
            public int productID { get; set; } 
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }
    }
}
