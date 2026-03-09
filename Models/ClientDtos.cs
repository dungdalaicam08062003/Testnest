namespace Websitecomputer.Models
{
    internal class ClientDtos
    {
        public record Client
        {
            public int AccountID { get; init; }
            public int ClientID { get; init; }
            public string ClientCode { get; init; } = "";
            public string ClientName { get; init; } = "";
            public string PhoneNumber { get; init; } = "";
            public string ClientAddess { get; set; } = "";
            public decimal TotalMoney { get; set; }
        }
        public record Cart
        {
            public int CartID { get; init; }
            public int ClientID { get; set; }
            public int CartItemID { get; set; }
            public int ProductID { get; set; }
            public decimal price { get; set; }

        }
       
    }
}
