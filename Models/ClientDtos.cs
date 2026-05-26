namespace WebsiteComputer.Models
{
    public class ClientDtos
    {
       
        public record ClientDetail
        {
            public int accountID { get; set; } 
            public string clientCode { get; set; } = "";
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public string clientName { get; set; } = "";
            public string phoneNumber { get; set; } = "0";
            public string clientAddress { get; set; } = "Address";
            public decimal totalMoney { get; set; } = 0;

        }
        public record ClientLogin(
            string email, 
            string password
            );
        public record ClientSignIn(
            string fullName,
            string email,
            string password
            );
        public class ClientInformation
        {
            public string? clientID { get; set; } 
            public string? clientName { get; set; } 
            public string? phoneNumber { get; set; } 
            public string? clientAddress { get; set; } 
            public decimal? totalMoney { get; set; } 
        }
        public class GetClientProfile()
        {
            public string name { get; set; } = "";
            public string email { get; set; } = "";
        }
        public class UpdateClientProfile()
        {
            public string name { get; set; } = "";
            public string gmail { get; set; } = "";
            public string currentPassword { get; set; } = "";
            public string newPassword { get; set; } = "";
        }
        public record Cart
        {
            public int cartID { get; init; }
            public int clientID { get; set; }
            public int cartItemID { get; set; }
            public int productID { get; set; }
            public decimal price { get; set; }

        }
       
    }
}
