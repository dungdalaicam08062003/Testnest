using WebsiteComputer.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Models;
using Npgsql;
using static WebsiteComputer.Models.AdminProduct;

namespace WebsiteComputer.Database
{
    public static class DBProductDetail
    {
        // public static async Task Main(string[] args)
        // {

        //     var builder = WebApplication.CreateBuilder(args);

        //     var config = new ConfigurationBuilder()
        //         .SetBasePath(Directory.GetCurrentDirectory())
        //         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //         .Build();
        //     var connStr = config.GetConnectionString("Supabase")
        //         ?? throw new InvalidOperationException("Missing ConnectionStrings:Supabase");
        //     var json = await ReadAsDtoAsync(connStr, "PRD001");
        //     Console.WriteLine(json);

        // }

        public static async Task<List<ProductSpec?>> GetProductSpecAsync(string connStr, string ProductCode)
        {
            var listSpec = new List<ProductSpec?>();
            try
            {
                using var conn = ConnectDB.ConnectSupabase(connStr);
                await conn.OpenAsync();

                var sql = @$"                  
                          SELECT
    ps.spec_key   AS speckey,
    ps.spec_value AS specvalue
FROM product_specs ps
INNER JOIN products p
    ON p.product_id = ps.product_id
WHERE p.product_code = @productCode;
                                                        ";
                await using var cmd = new NpgsqlCommand(sql, conn);

                cmd.Parameters.Add(new NpgsqlParameter("@productCode", SqlDbType.VarChar) { Value = ProductCode });

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {

                    listSpec.Add(new ProductSpec
                    {
                        SpecKey = reader.GetString(reader.GetOrdinal("speckey")),
                        SpecValue = reader.GetString(reader.GetOrdinal("specvalue"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return listSpec;
        }

        public static async Task<ProductMainInfo?> GetProductMainAsync(string connStr, string ProductCode)
        {
            ProductMainInfo? info = null;
            try
            {
                using var conn = ConnectDB.ConnectSupabase(connStr);
                await conn.OpenAsync();
                var sql = @$"


SELECT
    p.product_name AS name,
    p.price        AS price,
    p.stock        AS stock,
    b.brand_name   AS brand,
    CASE
        WHEN p.discount_id IS NULL THEN p.price
        ELSE p.price * (1 - di.discount_value)
    END AS price_after_discount,
    p.discount_id AS discount_id,
    (
        SELECT i.image_url
        FROM product_images i
        WHERE i.product_id = p.product_id
        ORDER BY i.sortorder ASC, i.image_id ASC
        LIMIT 1
    ) AS thumbnail
FROM products p
LEFT JOIN brands b ON b.brand_id = p.brand_id
LEFT JOIN discount di ON p.discount_id = di.discount_id
WHERE p.product_code = @product_code;


";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.Add(new NpgsqlParameter("@product_code", SqlDbType.VarChar) { Value = ProductCode });
                await using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                    {
                        info = new ProductMainInfo
                        {
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Price = reader.GetDecimal(reader.GetOrdinal("price")),
                            priceAfterDiscount = reader.GetDecimal(reader.GetOrdinal("price_after_discount")),
                            Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                            Brand = reader.IsDBNull(reader.GetOrdinal("brand"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("brand")),
                            Thumbnail = reader.IsDBNull(reader.GetOrdinal("thumbnail"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("thumbnail")),
                            VoucherID = reader.IsDBNull(reader.GetOrdinal("discount_id"))
                                        ? null
                                        : reader.GetInt32(reader.GetOrdinal("discount_id"))
                        };
                    }

            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return info;
        }
        public static async Task<List<string>> GetImageAsync(string connStr, string ProductCode)
        {
            var result = new List<string>();
            var productId = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.ConnectSupabase(connStr);
                await conn.OpenAsync();

                var sql = @"
SELECT i.image_url
FROM product_images i
WHERE i.product_id = @ProductId
  AND i.sortorder > 1
ORDER BY i.sortorder ASC, i.image_id ASC;

                            ";
                await using var cmd = new NpgsqlCommand(sql, conn);

                cmd.Parameters.Add(new NpgsqlParameter("@ProductId", SqlDbType.VarChar) { Value = productId });

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        var url = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(url))
                            result.Add(url);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return result;
        }
        public static async Task<ProductDetail?> ReadAsDtoAsync(string connStr, string ProductCode)
        {
            var main = await GetProductMainAsync(connStr, ProductCode);
            if (main is null) return null;

            var specs = await GetProductSpecAsync(connStr, ProductCode);
            var images = await GetImageAsync(connStr, ProductCode);

            return new ProductDetail
            {
                id = ProductCode,
                Name = main.Name,
                Price = main.Price,
                priceAfterDiscount = main.priceAfterDiscount,
                Stock = main.Stock,
                Brand = main.Brand,
                Thumbnail = main.Thumbnail,
                Specs = specs,
                Images = images,
                VoucherID = main.VoucherID
            };
        }

        public static async Task<List<ProductGetList>> GetListProductForAdminPage(string connStr, ProductGetList productItem )
        {
            var list = new List<ProductGetList>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"SELECT [ProductCode] as productCode 
                                  ,[ProductName] as productName
                                  ,[Price] as price
                                  ,[Stock] as stock
                              FROM [dbo].[Products]
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    list.Add(
                        new ProductGetList
                        {
                            productCode = reader.GetString(reader.GetOrdinal("productCode")),
                            productName = reader.GetString(reader.GetOrdinal("productName")),
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            stock = reader.GetInt32(reader.GetOrdinal("productCode"))
                        });
                }
            }
            catch 
            {
                throw;
            }
            return list;
        }
        public static async Task<int?> createProduct(string connStr, CreateUpdateProduct productInfo, ProductSpec productSpec)
        {
            int? ProductID = null;
            try
            {
                var now = DateTime.UtcNow;
                var productCode = $"PRO-{now:yyyymmdd}-{Random.Shared.Next(1000, 9999)}";
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
                cmd.Parameters.Add(new SqlParameter("@SpecKey", SqlDbType.NVarChar) { Value = productSpec.SpecKey });
                cmd.Parameters.Add(new SqlParameter("@Specvalue", SqlDbType.NVarChar) { Value = productSpec.SpecValue });
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
            return ProductID;
        }
        public static async Task<int?> updateProductDetail(string connStr, ProductItem productItem, ProductSpec productSpec)
        {
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, productItem.id);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ProductID;
        }
        public static async Task<int?> deleteProductDetail(string connStr, string productCode)
        {
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, productCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            BEGIN TRY
                                BEGIN TRAN;

                                DELETE FROM dbo.ProductImages
                                WHERE ProductID = @productID;

                                DELETE FROM dbo.ProductSpecs
                                WHERE ProductID = @productID;

                                DELETE FROM dbo.Products
                                WHERE ProductID = @productID;

                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0
                                    ROLLBACK TRAN;

                                THROW; -- trả lỗi ra ngoài
                            END CATCH;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@productID", SqlDbType.Int) { Value = ProductID });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ProductID;
        }
        
        //return object
        
        // return string Json
    }
}

