using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using static WebsiteComputer.Models.AdminOrder;
namespace WebsiteComputer.Database.DBAdmin
{
    public class DBOrder
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
        //    var a = await GetOrderDetail(connStr, "ORD-20260316-8606");
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.WriteLine(json);
        //}
        public static async Task<List<GetOrderList>> GetOrderList(string conStr)
        {
            var list = new List<GetOrderList>();
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT[OrderCode] as orderCode
                                  ,cl.ClientName as clientName
                                  ,[CreateAt]	as createAt
                                  ,o.[Address]	as addressOrder
                                  ,o.[PhoneNumber] as phoneNumber
                              FROM [dbo].[Orders] as o
                              Inner Join dbo.Client as cl
                              on cl.ClientID = o.ClientID";
                await using var cmd = new SqlCommand(sql, conn);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    list.Add(new GetOrderList { 
                        orderID = reader.GetString(reader.GetOrdinal("orderCode")),
                        clientName = reader.GetString(reader.GetOrdinal("clientName")),
                        Address = reader.GetString(reader.GetOrdinal("addressOrder")),
                        phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                        creatAt = reader.GetDateTime(reader.GetOrdinal("createAt"))
                    });
                }
            }
            catch
            {
                throw;
            }
            return list;
        }
        public static async Task<OrderDetail> GetOrderDetail(string conStr, string orderCodeDetail)
        {
            var orderId = await ConnectDB.GetOrderIDFromOrderCode(conStr, orderCodeDetail);
            OrderDetail? orderDetail = new OrderDetail();
            var listOrder = new List<OrderItems>(); 
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT 
                                  o.[TotalPrice] as totalPrice
                                  ,[CreateAt] as createAt
                                  ,[Address] as addressOrder
                                  ,o.[PhoneNumber] as phoneNumber
                                  ,o.TotalPrice as totalMoney
	                              ,ClientName as clientName
                              FROM [dbo].[Orders] as o
                              left join Client as cl 
                              on o.ClientID = cl.ClientID 
                              where o.OrderID = @OrderID

                            Select  
                                    oi.OrderID as orderID, 
                                    ProductName as productName
		                            ,Quantity  as quantity,
		                            oi.Price as price 
		                            ,oi.TotalPrice as orderItemTotalPrice
                            from OrderItems as oi
                            left join Products as p 
                            on p.ProductID = oi.ProductID
                            where oi.OrderID = @OrderID";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", SqlDbType.Int) { Value = orderId });
                var reader = await cmd.ExecuteReaderAsync();
                
                if ( await reader.ReadAsync())
                {
                    orderDetail = new OrderDetail
                    {
                        order = new GetOrderList
                        {
                            orderID = orderCodeDetail,
                            clientName = reader.GetString(reader.GetOrdinal("clientName")),
                            phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                            Address = reader.GetString(reader.GetOrdinal("addressOrder")),
                            creatAt = reader.GetDateTime(reader.GetOrdinal("createAt")),
                            totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney"))
                        }

                    };
                }
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    listOrder.Add(new OrderItems
                    {
                        productName = reader.GetString(reader.GetOrdinal("productName")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                        totalPrice = reader.GetDecimal(reader.GetOrdinal("orderItemTotalPrice"))
                        
                    });
                }

            }
            catch
            {
                throw;
            }
            if (orderDetail != null)
            {
                orderDetail.listOrderItem = listOrder;
            }
            return orderDetail;
            
        }
    }
}
