using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using WebsiteComputer.Database;
using WebsiteComputer.Models.Policy;
using static WebsiteComputer.Models.Policy.Discount;
namespace WebsiteComputer.Database.DBAdmin
{
    public class DBAdminDiscount
    {
        public static async Task<CreateDiscountPolicy> CreateDiscount(string connStr, CreateDiscountPolicy newDiscount) 
        {
            try 
            {

                var now = DateTime.UtcNow;
                var discountCodeValue = $"DI-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                newDiscount.discountCode = discountCodeValue;
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            INSERT INTO [dbo].[Discount]
                            (
                                [DiscountName],
                                [DiscountCode],
                                [DiscountValue],
                                [DateStart],
                                [DateEnd]
                            )
                            VALUES
                            (
                                @discountName,
                                @discountCode,
                                @discountValue,
                                @dateStart,
                                @dateEnd
                            );
                            


                            select [DiscountID] as discountID
                                  ,[DiscountName] as disountName
                                  ,[DiscountValue] as discountValue
                                  ,[DateStart] as dateStart
                                  ,[DateEnd]	as dateEnd
                                  ,[DiscountCode] as discountCode
                            from dbo.Discount where [DiscountCode] = @discountCode
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountName", SqlDbType.NVarChar) { Value = newDiscount.discountName});
                cmd.Parameters.Add(new SqlParameter("@discountCode", SqlDbType.NVarChar) { Value = discountCodeValue });
                cmd.Parameters.Add(new SqlParameter("@discountValue", SqlDbType.Decimal) { Value = newDiscount.discountValue });
                cmd.Parameters.Add(new SqlParameter("@dateStart", SqlDbType.DateTime2) { Value = newDiscount.dateStart });
                cmd.Parameters.Add(new SqlParameter("@dateEnd", SqlDbType.DateTime2) { Value = newDiscount.dateEnd });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    newDiscount.discountCode = reader.GetString(reader.GetOrdinal("discountCode"));
                    newDiscount.discountName = reader.GetString(reader.GetOrdinal("disountName"));
                    newDiscount.discountValue = reader.GetDecimal(reader.GetOrdinal("discountValue"));
                    newDiscount.dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart"));
                    newDiscount.dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"));
                }
            }
            catch
            {
                throw  ;
            }
            return newDiscount;
        }
        public static async Task<Discount.DiscountPolicy> ReadDiscount(string connStr, string code)
        {
            var discountPolicy = new DiscountPolicy();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            select [DiscountID] as discountID
                                  ,[DiscountName] as disountName
                                  ,[DiscountValue] as discountValue
                                  ,[DateStart] as dateStart
                                  ,[DateEnd]	as dateEnd
                                  ,[DiscountCode] as discountCode
                            from dbo.Discount where [DiscountCode] = @code

                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@code", SqlDbType.VarChar) { Value = code });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    discountPolicy.discountID =  reader.GetString(reader.GetOrdinal("discountCode"));
                    discountPolicy.discountName = reader.GetString(reader.GetOrdinal("disountName"));
                    discountPolicy.discountValue = reader.GetDecimal(reader.GetOrdinal("discountValue"));
                    discountPolicy.dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart"));
                    discountPolicy.dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"));
                }
            }
            catch
            {
                throw;
            }
            return discountPolicy;
        }
        public static async Task<List<Discount.DiscountPolicy>> ReadListDiscount(string connStr)
        {
            Discount.DiscountPolicy discountPolicy;
            var listDiscountPolicy = new List<DiscountPolicy>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"SELECT [DiscountID] as discountID
                          ,[DiscountName] as disountName
                          ,[DiscountValue] as discountValue
                          ,[DateStart] as dateStart
                          ,[DateEnd]	as dateEnd
                          ,[DiscountCode] as discountCode
                      FROM [dbo].[Discount]


";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listDiscountPolicy.Add(discountPolicy = new DiscountPolicy()
                    {
                        discountID =  reader.GetString(reader.GetOrdinal("discountCode")),
                        discountName = reader.GetString(reader.GetOrdinal("disountName")),
                        discountValue = reader.GetDecimal(reader.GetOrdinal("discountValue")),
                        dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart")),
                        dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"))
                    });

                }
            }
            catch 
            {
                throw;
            }
            return listDiscountPolicy;
        }
        public static async Task<Discount.DiscountPolicy> UpdateDiscount(string connStr, Discount.UpdateDiscountPolicy discountPolicy)
        {
            var newDiscountUpdate = new DiscountPolicy();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"UPDATE [dbo].[Discount]
                           SET [DiscountName] = @discountName
                              ,[DiscountValue] = @discountValue
                              ,[DateStart] = @dateStart 
                              ,[DateEnd] = @dateEnd 
                         WHERE DiscountCode = @discountCode
                            select [DiscountID] as discountID
                                  ,[DiscountName] as discountName
                                  ,[DiscountValue] as discountValue
                                  ,[DateStart] as dateStart
                                  ,[DateEnd]	as dateEnd
                                  ,[DiscountCode] as discountCode
                            from dbo.Discount where [DiscountCode] = @discountCode
                        ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountName", SqlDbType.NVarChar) { Value = discountPolicy.discountName });
                cmd.Parameters.Add(new SqlParameter("@discountCode", SqlDbType.NVarChar) { Value = discountPolicy.discountCode });
                var p = cmd.Parameters.Add(new SqlParameter("@discountValue", SqlDbType.Decimal) { Value = discountPolicy.discountValue });
                p.Precision = 18;
                p.Scale = 2;

                cmd.Parameters.Add(new SqlParameter("@dateStart", SqlDbType.DateTime2) { Value = discountPolicy.dateStart });
                cmd.Parameters.Add(new SqlParameter("@dateEnd", SqlDbType.DateTime2) { Value = discountPolicy.dateEnd });
                using var reader = await cmd.ExecuteReaderAsync(); 

                if (await reader.ReadAsync())
                {
                    newDiscountUpdate.discountID = reader.GetString(reader.GetOrdinal("discountCode"));
                    newDiscountUpdate.discountName = reader.GetString(reader.GetOrdinal("discountName"));
                    newDiscountUpdate.discountValue = reader.GetDecimal(reader.GetOrdinal("discountValue"));
                    newDiscountUpdate.dateStart = reader.GetDateTime(reader.GetOrdinal("dateStart"));
                    newDiscountUpdate.dateEnd = reader.GetDateTime(reader.GetOrdinal("dateEnd"));
                }
            }
            catch 
            {
                throw;
            }
            return newDiscountUpdate;
        }
        public static async Task<string> DeleteDiscount(string connStr, string discountCode)
        {
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"DELETE FROM [dbo].[Discount]
                            WHERE DiscountCode = @code";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@code", SqlDbType.VarChar) { Value = discountCode });
                var affect = await cmd.ExecuteNonQueryAsync();
            }
            catch 
            {
                throw;
            }
            return discountCode;
        }
        public static async Task<int> GeDiscountIDFromDiscountCode(string connStr, string Code)
        {
            int discountID = 1;
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"select 
                            p.DiscountID as discountID
                            from dbo.Discount as p
                            where p.DiscountCode = @code";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@code", SqlDbType.VarChar) { Value = Code });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    discountID = reader.GetInt32(reader.GetOrdinal("discountID"));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return discountID;
        }
        public static async Task<bool> applyDicountProductCode(string connStr, string code, string discountCode) {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                    begin tran 
                    UPDATE [dbo].[Products]
                       SET [DiscountID] = @discountID
                    WHERE ProductCode = @productCode
                    commit tran 
                ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                cmd.Parameters.Add(new SqlParameter("@productCode", SqlDbType.VarChar) { Value = code});
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> applyDicountAll(string connStr, string discountCode)
        {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                   begin tran 
                   UPDATE [dbo].[Products]
                       SET [DiscountID] = @discountID
                   commit tran 
                ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> applyDicountBrand(string connStr, int brandID, string discountCode)
        {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            begin tran
                            UPDATE [dbo].[Products]
                               SET [DiscountID] = @discountID
                            WHERE BrandID = @brandID
                            commit tran 
                ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                cmd.Parameters.Add(new SqlParameter("@brandID", SqlDbType.Int) { Value = brandID });
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> applyDicountCategorate(string connStr, string categoryID, string discountCode)
        {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            begin tran
                            UPDATE [dbo].[Products]
                               SET [DiscountID] = @discountID
                             WHERE CategoryID = @CategoryID
                             commit tran 
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                cmd.Parameters.Add(new SqlParameter("@CategoryID", SqlDbType.Int) { Value = categoryID });  
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> applyDicountClient(string connStr,  string discountCode)
        {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            UPDATE [dbo].Client
                               SET [DiscountID] = @DicountID
                             WHERE TotalMoney > 100000000
                            GO
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static async Task<bool> applyDicountOrder(string connStr, string discountCode)
        {
            int discountID = await GeDiscountIDFromDiscountCode(connStr, discountCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            UPDATE [dbo].Client
                               SET [DiscountID] = @DicountID
                             WHERE TotalMoney > 100000000
                            GO
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@discountID", SqlDbType.Int) { Value = discountID });
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
