using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using WebsiteComputer.Models.Policy;
using static WebsiteComputer.Models.Policy.Guarantee;

namespace Database.DBAdmin
{
    public class DBAdminGuarantee
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
        //    //OrderItemRequest orderItemRequest = new OrderItemRequest("P001", 2);
        //    //OrderItemRequest orderItemRequest2 = new OrderItemRequest("P001", 2);
        //    //List<OrderItemRequest> a = new List<OrderItemRequest>();
        //    //a.Add(orderItemRequest);
        //    //a.Add(orderItemRequest2);
        //    //CreateOrderRequest createOrderRequest = new CreateOrderRequest("CLI-0003", "Da nang", "0987383939", a);
        //    //var i = await addProductOrderByCart(connStr, createOrderRequest);
        //    var a = await ReadGuarantee(connStr, "Gua_6");
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.OutputEncoding = System.Text.Encoding.UTF8;

        //    Console.WriteLine(json);

        //}
        public static async Task<bool> CreateGuarantee(string connStr, GuaranteeProduct guranteeInfo){
            try {
                var now = DateTime.UtcNow;
                guranteeInfo.guaranteeID = $"GUA-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            begin tran
                            INSERT INTO [dbo].[Guarantee]
                                       ([ProductID]
                                       ,[DateStart]
                                       ,[DateEnd]
                                       ,[guaranteeCode])
                                 VALUES
                                       (@productID
                                       ,@dateStart
                                       ,@dateEnd
                                       ,@guaranteCode)
                            commit tran
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@guaranteCode", SqlDbType.VarChar){ Value = guranteeInfo.guaranteeID});
                cmd.Parameters.Add(new SqlParameter("@productID", SqlDbType.Int){ Value = guranteeInfo.productID});
                cmd.Parameters.Add(new SqlParameter("@dateStart", SqlDbType.DateTime2){ Value = guranteeInfo.dateStart});
                cmd.Parameters.Add(new SqlParameter("@dateEnd", SqlDbType.DateTime2){ Value = guranteeInfo.dateEnd});
                await cmd.ExecuteNonQueryAsync();

            }
            catch {
                return false;
            }
            return true;
        }
        public static async Task<GuaranteeProduct> ReadGuarantee(string connStr, string id){
            var guranteeInfo = new GuaranteeProduct();
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT[ProductID] as productID 
                                  ,[DateStart] as dateStart
                                  ,[DateEnd] as dateEnd
                                  ,[guaranteeCode] as code
                              FROM [dbo].[Guarantee]
                              where [guaranteeCode] = @code
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@code", SqlDbType.VarChar){ Value = id });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    guranteeInfo.guaranteeID = reader.GetString(reader.GetOrdinal("code"));
                    guranteeInfo.productID = reader.GetInt32(reader.GetOrdinal("productID"));
                    guranteeInfo.dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart"));
                    guranteeInfo.dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"));
                }
            }
            catch {
                throw ;
            }
            return guranteeInfo;
        }
        public static async Task<List<GuaranteeProduct>> ReadListGuarantee(string connStr){
            var listGuranteeInfo = new List<GuaranteeProduct>();
            GuaranteeProduct guranteeInfo;
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT [GuaranteeID] 
                                  ,[ProductID]	as productID
                                  ,[DateStart] as dateStart
                                  ,[DateEnd] as dateEnd
                                  ,[guaranteeCode] as guaranteeID
                              FROM [dbo].[Guarantee]";
                await using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listGuranteeInfo.Add( guranteeInfo = new GuaranteeProduct()
                        {
                            guaranteeID = reader.GetString(reader.GetOrdinal("guaranteeID")),
                            productID = reader.GetInt32(reader.GetOrdinal("productID")),
                            dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart")),
                            dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"))
                        }
                    );
                }
            }
            catch {
                throw ;
            }
            return listGuranteeInfo;
        }
        public static async Task<bool> UpdateGuarantee(string connStr, GuaranteeProduct guranteeInfo){
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            begin tran 
                            UPDATE [dbo].[Guarantee]
                               SET [ProductID] = @productID
                                  ,[DateStart] = @DateStart
                                  ,[DateEnd] = @DateEnd
                             WHERE [guaranteeCode] = @guranteeID
                            commit tran
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@guranteeID", SqlDbType.VarChar){ Value = guranteeInfo.guaranteeID});
                cmd.Parameters.Add(new SqlParameter("@productID", SqlDbType.VarChar){ Value = guranteeInfo.productID});
                cmd.Parameters.Add(new SqlParameter("@DateStart", SqlDbType.DateTime2){ Value = guranteeInfo.dateStart});
                cmd.Parameters.Add(new SqlParameter("@DateEnd", SqlDbType.DateTime2){ Value = guranteeInfo.dateEnd});
                await cmd.ExecuteNonQueryAsync();
                
            }
            catch {
                return false ;
            }
            return true;
        }
        public static async Task<bool> DeleteGuarantee(string connStr, string id){
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"DELETE FROM [dbo].[Guarantee]
                            WHERE guaranteeCode = @guaranteeCode";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@guaranteeCode", SqlDbType.VarChar){ Value = id});
                await cmd.ExecuteNonQueryAsync();
            }
            catch {
                return false ;
            }
            return true ;
        }
    }
}
