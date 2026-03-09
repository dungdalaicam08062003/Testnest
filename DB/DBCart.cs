using ConsoleApp1.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using Websitecomputer.Page;
using static Websitecomputer.Models.ClientDtos;

namespace Websitecomputer.DB
{
    public class DBCart
    {
        //public static async Task Main(string[] args)
        //{

        //    var builder = WebApplication.CreateBuilder(args);

        //    var config = new ConfigurationBuilder()
        //       .SetBasePath(Directory.GetCurrentDirectory())
        //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //       .Build();
        //    var connStr = config.GetConnectionString("Default")
        //        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        //    var json = await ReadAsJsonAsync(connStr, "P001");
        //    Console.WriteLine(json);
        //   await AddProductToCart(connStr, "P001", "CLI-0001", 5);

        //}

        public static async Task<int?> GetCart(string connStr, string ClientCode)
        {
            var ClientID = await ConnectDB.GetClientIDFromClientCode(connStr, ClientCode);
            int? CartID = 0 ;
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            Select 
	                            c.CartID as CartID
                            From dbo.Cart AS c
                            where c.ClientID = @ClientID
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) {Value = ClientID});
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync()) {
                    CartID = reader.GetInt32(reader.GetOrdinal("CartID"));                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("cant't connection");
                Console.WriteLine(e.Message);
            }
            return CartID;
        }
        public static async Task AddProductToCart(string connStr, string productCode, string clientCode, int quantity)
        {
            try
            {
                var productID = await ConnectDB.GetProductIDFromProductCode(connStr, productCode);
                var CartID = await GetCart(connStr, clientCode);
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"update dbo.CartItem
                            set Quantity = Quantity + @Quantity 
                            Where CartID = @CartID and ProductID = @ProductID;

                            IF @@ROWCOUNT = 0

                            BEgin 
	                            declare @price decimal(18,2);
	                            select @price = p.price 
	                            from dbo.Products as p
	                            where p.ProductID = @ProductID  ;
                                
	                            Insert into dbo.CartItem(CartID,ProductID,Quantity, Price)
	                            values(@CartID, @ProductID, @Quantity, @price);
                            end";
                
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@CartID", SqlDbType.Int) { Value = CartID });
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productID });
                cmd.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity });

                var affected = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Thực thi xong, rows affected (tính cả UPDATE/INSERT): {affected}");

                Console.WriteLine("da thuc thi");
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't Connection");
                Console.WriteLine(e.Message);
            }
        }
        public static async Task<List<CartItem?>> GetCartItem(string connStr, int CartID)
        {
            List<CartItem?> listCartItem = new List<CartItem?>() ;
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            use WebsiteSellLaptop;
                            Select 
                                 c.CartID as CartID,
                                 c.CartItemID as CartItemID,
	                             c.ProductID as ProductID,
	                             c.Quantity as Quantity,
	                             p.ProductName as Productname,
	                             pi.ImageURL as Thumbnail,
	                             c.Price as Price
                             From dbo.CartItem AS c
                             left join dbo.Products as p ON p.ProductID = c.ProductID
                             OUTER APPLY (
                                SELECT TOP (1) i.ImageUrl
                                FROM dbo.ProductImages AS i
                                WHERE i.ProductId = p.ProductId
                                ORDER BY i.SortOder ASC, i.ImageId ASC
                            ) AS pi
                             where c.CartID = 1
                        ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value =CartID });
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listCartItem.Add(new CartItem
                    {
                        cartID = reader.GetInt32(reader.GetOrdinal("CartID")),
                        cartItemID = reader.GetInt32(reader.GetOrdinal("CartItemID")),
                        productID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                        quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        productName = reader.GetString(reader.GetOrdinal("ProductName")),
                        price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        thumbnail =reader.GetString(reader.GetOrdinal("Thumbnail"))
                    });
                }
            } 
            catch(Exception e) 
            { 
                Console.WriteLine("Can't connection"); 
            }

            return listCartItem;
        }
        public static async Task<List<CartItem?>> ReadAsJsonAsync(string connStr, string clientCode)
        {
            var CartID = await GetCart(connStr, clientCode);
            var listCartItem = await GetCartItem(connStr, Convert.ToInt32(CartID));
            var json = JsonSerializer.Serialize(listCartItem, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            return listCartItem;
        }
    }
}
