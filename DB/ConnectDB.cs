using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsoleApp1.DB
{
    public static class ConnectDB
    {
        public static SqlConnection Create(string connectionString)
            => new SqlConnection(connectionString);
        public static async Task<int> GetProductIDFromProductCode(string connStr,string productCode)
        {
            int productID = 1;
            try 
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"select 
                            p.ProductID as productID
                            from dbo.Products as p
                            where p.productCode = @ProductCode";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ProductCode", SqlDbType.VarChar) { Value = productCode });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    productID = reader.GetInt32(reader.GetOrdinal("productID"));
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
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"select 
                            p.ClientID as ClientID
                            from dbo.Client as p
                            where p.ClientCode = @ClientCode";
                await using var cmd = new SqlCommand(sql, conn);
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
    }
}
