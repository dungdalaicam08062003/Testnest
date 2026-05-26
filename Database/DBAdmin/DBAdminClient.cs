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
using static WebsiteComputer.Models.AdminManageClient;
using static WebsiteComputer.Models.AdminProduct;

namespace WebsiteComputer.Database.DBAdmin
{
    public class DBAdminClient
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
        //    var a = await GetClientDetail(connStr,4);
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.WriteLine(json);
        //}
        public static async Task<List<ClientMainInfo>> GetListClient(string conStr)
        {
            var list = new List<ClientMainInfo>();
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT cl.[ClientID] as clientID
                                  ,[ClientName] as clientname
                                  ,cl.[PhoneNumber] as phoneNumber
                                  ,[ClientAddress] as clientAddress
                                  ,Count(o.ClientID) as totalOrder
                            FROM [dbo].[Client] as cl
	                            left join dbo.Orders as o on cl.ClientID = o.ClientID 
                            group by 
	                            cl.ClientID,
	                            cl.ClientName,
	                            cl.PhoneNumber,
	                            cl.ClientAddress;";
                await using var cmd = new SqlCommand(sql, conn);
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new ClientMainInfo
                    {
                        ClientID = reader.GetInt32(reader.GetOrdinal("clientID")),
                        ClientName = reader.GetString(reader.GetOrdinal("clientname")),
                        PhoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                        ClientAddress = reader.GetString(reader.GetOrdinal("clientAddress")),
                        TotalOrder = reader.GetInt32(reader.GetOrdinal("totalOrder"))

                    });
                }
            }
            catch
            {
                throw;
            }
            return list;
        }
        public static async Task<AdminClientDetail?> GetClientDetail(string conStr, int ClientID)
        {
            AdminClientDetail? adminClientDetail = null;
            var list = new List<AdminClientOrder?>(); 
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT
                                cl.ClientName        AS clientName,
                                cl.PhoneNumber       AS phoneNumber,
                                cl.ClientAddress     AS clientAddress,
                                o.OrderCode          AS orderCode,
                                o.CreateAt           AS createAt,
                                o.TotalPrice         AS totalPrice,
                                o.StatusOrders       AS statusOrder
                            FROM dbo.Client cl
                            LEFT JOIN dbo.Orders o ON cl.ClientID = o.ClientID
                            WHERE cl.ClientID = @clientID
                            ORDER BY o.CreateAt DESC;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@clientID", SqlDbType.Int) { Value = ClientID });
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (adminClientDetail == null)
                    {
                        adminClientDetail = new AdminClientDetail
                        {
                            ClientName = reader.GetString(reader.GetOrdinal("clientName")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                            ClientAddress = reader.GetString(reader.GetOrdinal("clientAddress")),
                            adminClientDetails = list
                        };
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("orderCode")))
                    {
                        list.Add(new AdminClientOrder
                        {
                            orderCode = reader.GetString(reader.GetOrdinal("orderCode")),
                            createAt = reader.GetDateTime(reader.GetOrdinal("createAt")),
                            totalPrice = reader.GetDecimal(reader.GetOrdinal("totalPrice")),
                            status = reader.GetString(reader.GetOrdinal("statusOrder"))
                        });
                    }
                }

            }
            catch
            {
                throw;
            }
            if(adminClientDetail != null)
            {
                adminClientDetail.TotalOrder = list.Count;
            }
           
            return adminClientDetail;
        }
    }
}
