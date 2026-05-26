using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Npgsql;

namespace WebsiteComputer.Database
{
    public static class ConnectDB
    {
        public static SqlConnection Create(string connectionString)
            => new SqlConnection(connectionString);
        public static NpgsqlConnection ConnectSupabase(string connectionString)
            => new NpgsqlConnection(connectionString);
        public static async Task<int> GetProductIDFromProductCode(string connStr,string productCode)
        {
            int productID = 1;
            try 
            {
                using var conn = ConnectSupabase(connStr);
                await conn.OpenAsync();
                var sql = @"SELECT
                                p.product_id AS productid
                            FROM products p
                            WHERE p.product_code = @product_code;";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.Add(new NpgsqlParameter("@product_code", SqlDbType.VarChar) { Value = productCode });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    productID = reader.GetInt32(reader.GetOrdinal("productid"));
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
            return productID;    
        }
        public static async Task<int> GetClientIDFromClientCode(string connStr, string ClientCode)
        {
            int ClientID = 1;
            try
            {
                using var conn = ConnectDB.ConnectSupabase(connStr);
                await conn.OpenAsync();
                var sql = @"select 
                            p.ClientID as ClientID
                            from dbo.Client as p
                            where p.ClientCode = @ClientCode";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ClientCode", SqlDbType.VarChar) { Value = ClientCode });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ClientID = reader.GetInt32(reader.GetOrdinal("ClientID"));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ClientID;
        }
        public static async Task<int> GetOrderIDFromOrderCode(string connStr,string orderCode)
        {
            int orderID = 1;
            try
            {
                using var conn = ConnectDB.ConnectSupabase(connStr);
                await conn.OpenAsync();
                var sql = @"select 
                            o.OrderID as OrderID
                            from dbo.Orders as o
                            where o.OrderCode = @OrderCode";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderCode", SqlDbType.VarChar) { Value = orderCode });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    orderID = reader.GetInt32(reader.GetOrdinal("OrderID"));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return orderID;
        }
    }
}
