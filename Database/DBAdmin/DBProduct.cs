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
using WebsiteComputer.Models;
using static WebsiteComputer.Models.AdminProduct;

namespace WebsiteComputer.Database.DBAdmin
{
    public class DBProduct
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
        //    var a = await ProductGetList(connStr);
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.WriteLine(json);
        //}
        public static async Task<List<ProductGetList>> ProductGetList(string conStr)
        {
            var list = new List<ProductGetList>();
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT [ProductCode] as productCode
                                  ,[ProductName] as productName
                                  ,[Price]	as price
                                  ,[Stock] as stock
                              FROM [dbo].[Products]";
                await using var cmd = new SqlCommand(sql, conn);
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new ProductGetList
                    {
                        productCode = reader.GetString(reader.GetOrdinal("productCode")),
                        productName = reader.GetString(reader.GetOrdinal("productName")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        stock = reader.GetInt32(reader.GetOrdinal("stock")),
                        
                    });
                }
            }
            catch
            {
                throw;
            }
            return list;
        }
        public static async Task<String?> createProduct(string connStr, CreateUpdateProduct? productInfo, List<ProductSpec?> ProductSpecs)
        {
            int? ProductID = null;
            string productCode = "";
            try
            {
                var now = DateTime.UtcNow;
                productCode = $"PRO-{now:yyyymmdd}-{Random.Shared.Next(1000, 9999)}";
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            DECLARE
	                            @productID int		
                            BEGIN TRY
                                BEGIN TRAN;
                                -- Insert Product + lấy ProductID an toàn
	                            select @brandID = BrandID from Brands as b where b.BrandName = @brandName
	                            select @categoryID = CategoryID from Categories as ca where ca.CategoryName = @categoryName

                                INSERT INTO Products
                                (
                                    ProductCode,
                                    ProductName,
                                    Price,
                                    Descriptions,
                                    BrandID,
                                    CategoryID,
                                    Stock,
                                    Rating,
                                    CreateAt,
                                    UpdateAt
                                )
    
                                VALUES
                                (
                                    @productCode,
                                    @productName,
                                    @price,
                                    @description,
                                    @brandID,
                                    @categoryID,
                                    @stock,
                                    @rating,
                                    @CreateAt,
                                    @CreateAt
                                );
	                            SELECT @productID = ProductID from Products where ProductCode = @productCode
	                            INSERT INTO ProductSpecs
                                (
                                    SpecKey,
                                    SpecValue,
                                    ProductID
                                )
                                VALUES
                                (
                                    @SpecKey,
                                    @Specvalue,
                                    @productID
                                );

                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                THROW;
                            END CATCH;
                            SELECT ProductID as productID from Products where ProductCode = @productCode"";";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@productCode", SqlDbType.VarChar) { Value = productCode });
                cmd.Parameters.Add(new SqlParameter("@productName", SqlDbType.NVarChar) { Value = productInfo.Name });
                cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Decimal) { Value = productInfo.Price });
                cmd.Parameters.Add(new SqlParameter("@description", SqlDbType.NVarChar) { Value = productInfo.description });
                cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int) { Value = productInfo.Stock });
                cmd.Parameters.Add(new SqlParameter("@CreateAt", SqlDbType.NVarChar) { Value = now });
                cmd.Parameters.Add(new SqlParameter("@brandName", SqlDbType.NVarChar) { Value = productInfo.Brand });
                cmd.Parameters.Add(new SqlParameter("@categoryName", SqlDbType.NVarChar) { Value = productInfo.Category });
                for (int i = 0; i < ProductSpecs.Count(); i++)
                {
                    string sql1 = @"

                        DECLARE
	                            @productID int		
                            BEGIN TRY
                                BEGIN TRAN;
                               
	                            SELECT @productID = ProductID from Products where ProductCode = @productCode
	                            INSERT INTO ProductSpecs
                                (
                                    SpecKey,
                                    SpecValue,
                                    ProductID
                                )
                                VALUES
                                (
                                    @SpecKey,
                                    @Specvalue,
                                    @productID
                                );s

                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                THROW;
                            END CATCH;
                           
                       ";
                    await using var cmd1 = new SqlCommand(sql1, conn);
                    cmd1.Parameters.Add(new SqlParameter("@productCode", SqlDbType.VarChar) { Value = productCode });
                    var productSpec = new ProductSpec();
                    cmd1.Parameters.Add(new SqlParameter("@SpecKey", SqlDbType.NVarChar) { Value = productSpec.SpecKey });
                    cmd1.Parameters.Add(new SqlParameter("@Specvalue", SqlDbType.NVarChar) { Value = productSpec.SpecValue });
                    var affect = await cmd1.ExecuteNonQueryAsync();
                }
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ProductID = reader.GetInt32(reader.GetOrdinal("productID"));
                }

                int imageQuantity = productInfo.image.Count;
                for (int i = 1; i <= imageQuantity; i++)
                {
                    string sql2 = @"BEGIN TRY
                                    BEGIN TRAN;
                                    DECLARE 
                                        @productID INT
	                                SELECT @productID = ProductID from Products where ProductCode = @productCode
                                    -- Product Image
                                    INSERT INTO ProductImages
                                    (
                                        ProductID,
                                        ImageURL,
                                        SortOder
                                    )
                                    VALUES
                                    (
                                        @productID,
                                        @ImageUrl,
                                        @sortOder
                                    );


                                    COMMIT TRAN;
                                END TRY
                                BEGIN CATCH
                                    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                    THROW;
                                END CATCH;";
                    await using var cmd2 = new SqlCommand(sql2, conn);
                    cmd2.Parameters.Add(new SqlParameter("@ImageUrl", SqlDbType.NVarChar) { Value = productInfo.image[i - 1] });
                    cmd2.Parameters.Add(new SqlParameter("@sortOder", SqlDbType.Int) { Value = i });
                    var affect = await cmd2.ExecuteNonQueryAsync();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return productCode;
        }

    }
}

