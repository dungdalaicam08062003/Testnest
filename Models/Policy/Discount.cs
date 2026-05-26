using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models.Policy
{
    public class Discount
    {
        public record DiscountPolicy()
        {
            public string? discountID { get; set; } = "";
            public string? discountName { get; set; } = "";
            public decimal discountValue { get; set; }
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }
        public record CreateDiscountPolicy()
        {
            public string? discountCode { get; set; } = "";
            public string discountName { get; set; } = "";
            public decimal discountValue { get; set; }
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }
        public record UpdateDiscountPolicy()
        {
            public string discountCode { get; set; } = "";
            public string discountName { get; set; } = "";
            public decimal discountValue { get; set; }
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }
    }
}
