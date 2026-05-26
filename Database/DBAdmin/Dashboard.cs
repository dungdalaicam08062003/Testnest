using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static WebsiteComputer.Models.AdminDashBoard;
using static WebsiteComputer.Models.AdminManageClient;
namespace WebsiteComputer.Database.DBAdmin
{
    internal class Dashboard
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
        //    var a = await GetDashboard(connStr);
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.WriteLine(json);
        //}
        public static async Task<DashBoardInfomation> GetDashboard(string connStr)
        {
            var dashboardInfo = new DashBoardInfomation();
             
            var list = new List<DashBoardOrder>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT TOP (20)[OrderCode] as orderCode 
                                  ,cl.ClientName as clientName 
                                  ,[TotalPrice] as totalPrice
                                  ,[StatusOrders] as statusOrders
                                  ,[CreateAt] as createAt
                                  ,o.[PhoneNumber] as phoneNumber
                              FROM [WebsiteSellLaptop].[dbo].[Orders] as o
                              inner join client as cl on cl.ClientID = o.ClientID
                              order by CreateAt desc   
                            ";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new DashBoardOrder
                        {
                            orderCode = reader.GetString(reader.GetOrdinal("orderCode")),
                            clientName = reader.GetString(reader.GetOrdinal("clientName")),
                            phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                            status = reader.GetString(reader.GetOrdinal("statusOrders")),
                            createAt = reader.GetDateTime(reader.GetOrdinal("createAt")),
                            totalPrice = reader.GetDecimal(reader.GetOrdinal("totalPrice"))

                        });
                    }
                }
                    
                
                sql = @"Select
                        (select count(*) from Orders where CreateAt >= DATEADD(Hour, -24 , SYSDATETIME())) as totalOrderIn24hour,
                        (select count(*) from Orders) as totalOrderCreated,
                        (select count(*) from Products) as totalProduct,
                        (select count(*) from Products where stock < 5)as StockofProductSmall";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    if (await reader.ReadAsync())
                    {
                        int totalProduct1 = reader.GetInt32(reader.GetOrdinal("totalProduct"));
                        int totalOrder1 = reader.GetInt32(reader.GetOrdinal("totalOrderCreated"));
                        int totalOrderIn24hour1 = reader.GetInt32(reader.GetOrdinal("totalOrderIn24hour"));
                        int stockOfproductSmall1 = reader.GetInt32(reader.GetOrdinal("StockofProductSmall"));

                        dashboardInfo = new DashBoardInfomation
                        {
                            totalProduct = totalOrder1,
                            totalOrder = totalOrder1,
                            totalOrderIn24hour = totalOrderIn24hour1,
                            stockOfproductSmall = stockOfproductSmall1,
                            listDashBoardOrders = list

                        };
                    }
                }
                   
            }
            catch
            {
                throw;
            }
            return dashboardInfo;
        }
    }
}
