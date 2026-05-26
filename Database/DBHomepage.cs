using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Npgsql;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using WebsiteComputer.Models;

public static class DBHomepage
{
    // public static async Task Main(string[] args)
    // {

    //    var builder = WebApplication.CreateBuilder(args);

    //    var config = new ConfigurationBuilder()
    //       .SetBasePath(Directory.GetCurrentDirectory())
    //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //       .Build();
    //    var connStr = config.GetConnectionString("Supabase")
    //        ?? throw new InvalidOperationException("Missing ConnectionStrings:Supabase");
    //    //var json = await ReadAsJsonAsync(connStr, "P001");
    //    //Console.WriteLine(json);
    //    var a = await SelectAllProductsAsList(connStr);
    //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
    //    {
    //        WriteIndented = true,
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //    });
    //    Console.WriteLine(json);

    // }
    
    public static async Task<List<ProductItem>> SelectAllProductsAsList(string connStr)
    {
        var list = new List<ProductItem>();
        try
        {
            using var conn = ConnectDB.ConnectSupabase(connStr);
            await conn.OpenAsync();

            var sql = @"
                     
        SELECT
            p.product_code      AS ""Id"",
            p.product_name      AS ""Name"",
            p.price             AS ""Price"",
            CASE
                WHEN p.discount_id IS NULL THEN p.price
                ELSE p.price * (1 - di.discount_value)
            END                 AS ""priceAfterDiscount"",
            b.brand_code        AS ""Brand"",
            c.category_name     AS ""Category"",
            pi.image_url        AS ""Thumbnail"",
            p.stock             AS ""Stock"",
            p.create_at         AS ""CreateAt"",
            p.discount_id       AS ""discountID""
        FROM products p
        LEFT JOIN brands b
               ON b.brand_id = p.brand_id
        LEFT JOIN categories c
               ON c.category_id = p.category_id
        LEFT JOIN discount di
               ON di.discount_id = p.discount_id
        LEFT JOIN LATERAL (
            SELECT image_url
            FROM product_images i
            WHERE i.product_id = p.product_id
            ORDER BY i.sortorder, i.image_id
            LIMIT 1
        ) pi ON TRUE
        ORDER BY p.product_id;
                    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductItem
                {
                    id = reader.GetString(reader.GetOrdinal("Id")),
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    priceAfterDiscount = reader.GetDecimal(reader.GetOrdinal("priceAfterDiscount")),
                    brand = reader.GetString(reader.GetOrdinal("Brand")),
                    category = reader.GetString(reader.GetOrdinal("Category")),
                    thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    createAt = reader.GetDateTime(reader.GetOrdinal("CreateAt")),
                    voucherId = reader.IsDBNull(reader.GetOrdinal("discountID")) 
                                ? null 
                                : reader.GetInt32(reader.GetOrdinal("discountID")) 
                });
            }
        }
        catch 
        {
            throw;
        }
        return list;
    }
    public static async Task<List<ProductItem>> SelectAllProductsByCategory(string connStr, string categoryName)
    {
        var list = new List<ProductItem>();
        try
        {
            using var conn = ConnectDB.ConnectSupabase(connStr);
            await conn.OpenAsync();

            var sql = @"
                        SELECT
                            p.ProductCode    AS Id,
                            p.ProductName    AS Name,
                            p.Price          AS Price,
                            b.BrandName      AS Brand,
	                        c.CategoryName   AS Category,
                            pi.ImageUrl      AS Thumbnail,
                            p.Stock          AS Stock,
                            p.CreateAt       AS CreateAt
                        FROM dbo.Products p
                        LEFT JOIN dbo.Brands b
                                ON b.BrandId = p.BrandId
                        LEFT JOIN dbo.Categories c
                                ON c.CategoryID = p.CategoryID

                        OUTER APPLY (
                            SELECT TOP (1) i.ImageUrl
                            FROM dbo.ProductImages i
                            WHERE i.ProductId = p.ProductId
                            ORDER BY i.SortOder, i.ImageId
                        ) pi
                        Where c.CategoryName = @categoryName
                        ORDER BY p.ProductId;
                    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@categoryName", SqlDbType.VarChar) { Value = categoryName });
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductItem
                {
                    id = reader.GetString(reader.GetOrdinal("Id")),
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    brand = reader.GetString(reader.GetOrdinal("Brand")),
                    category = reader.GetString(reader.GetOrdinal("Category")),
                    thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    createAt = reader.GetDateTime(reader.GetOrdinal("CreateAt"))
                });
            }
        }
        catch
        {
            throw;
        }
        return list;
    }
  
    public static async Task<List<ProductItem>> SelectAllProductsByBrand(string connStr, string brandName)
    {
        var list = new List<ProductItem>();
        try
        {
            using var conn = ConnectDB.ConnectSupabase(connStr);
            await conn.OpenAsync();

            var sql = @"
                        SELECT
                            p.ProductCode    AS Id,
                            p.ProductName    AS Name,
                            p.Price          AS Price,
                            b.BrandName      AS Brand,
	                        c.CategoryName   AS Category,
                            pi.ImageUrl      AS Thumbnail,
                            p.Stock          AS Stock,
                            p.CreateAt       AS CreateAt
                        FROM dbo.Products p
                        LEFT JOIN dbo.Brands b
                                ON b.BrandId = p.BrandId
                        LEFT JOIN dbo.Categories c
                                ON c.CategoryID = p.CategoryID

                        OUTER APPLY (
                            SELECT TOP (1) i.ImageUrl
                            FROM dbo.ProductImages i
                            WHERE i.ProductId = p.ProductId
                            ORDER BY i.SortOder, i.ImageId
                        ) pi
                        Where b.BrandName = @brandName
                        ORDER BY p.ProductId;
                    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add(new NpgsqlParameter("@brandName", SqlDbType.VarChar) { Value = brandName });
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductItem
                {
                    id = reader.GetString(reader.GetOrdinal("Id")),
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    brand = reader.GetString(reader.GetOrdinal("Brand")),
                    category = reader.GetString(reader.GetOrdinal("Category")),
                    thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    createAt = reader.GetDateTime(reader.GetOrdinal("CreateAt"))
                });
            }
        }
        catch
        {
            throw;
        }
        return list;
    }
    public static async Task<List<ProductItem>> SelectAllProductsHomepage(string connStr, string? brandName, string? category)
    {
        var list = new List<ProductItem>();
        try
        {
            using var conn = ConnectDB.Create(connStr);
            await conn.OpenAsync();

            var sql = @"
                        SELECT
                            p.ProductCode    AS Id,
                            p.ProductName    AS Name,
                            p.Price          AS Price,
                            b.BrandName      AS Brand,
	                        c.CategoryName   AS Category,
                            pi.ImageUrl      AS Thumbnail,
                            p.Stock          AS Stock,
                            p.CreateAt       AS CreateAt
                        FROM dbo.Products p
                        LEFT JOIN dbo.Brands b
                                ON b.BrandId = p.BrandId
                        LEFT JOIN dbo.Categories c
                                ON c.CategoryID = p.CategoryID

                        OUTER APPLY (
                            SELECT TOP (1) i.ImageUrl
                            FROM dbo.ProductImages i
                            WHERE i.ProductId = p.ProductId
                            ORDER BY i.SortOder, i.ImageId
                        ) pi

                          WHERE
                          (@Brand IS NULL OR b.BrandName  = @Brand)
                          AND
                          (@Category IS NULL OR c.CategoryName = @Category)
                        ORDER BY p.ProductId;";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Brand", (object?)brandName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Category", (object?)category ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductItem
                {
                    id = reader.GetString(reader.GetOrdinal("Id")),
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    brand = reader.GetString(reader.GetOrdinal("Brand")),
                    category = reader.GetString(reader.GetOrdinal("Category")),
                    thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    createAt = reader.GetDateTime(reader.GetOrdinal("CreateAt"))
                });
            }
        }
        catch
        {
            throw;
        }
        return list;
    }
    
    public static async Task<List<ProductItem>> SelectAllProductsByProductName(string connStr, string productName)
    {
        var list = new List<ProductItem>();
        try
        {
            using var conn = ConnectDB.Create(connStr);
            await conn.OpenAsync();

            var sql = @"
                        SELECT
                            p.ProductCode    AS Id,
                            p.ProductName    AS Name,
                            p.Price          AS Price,
                            b.BrandName      AS Brand,
	                        c.CategoryName   AS Category,
                            pi.ImageUrl      AS Thumbnail,
                            p.Stock          AS Stock,
                            p.CreateAt       AS CreateAt
                        FROM dbo.Products p
                        LEFT JOIN dbo.Brands b
                                ON b.BrandId = p.BrandId
                        LEFT JOIN dbo.Categories c
                                ON c.CategoryID = p.CategoryID

                        OUTER APPLY (
                            SELECT TOP (1) i.ImageUrl
                            FROM dbo.ProductImages i
                            WHERE i.ProductId = p.ProductId
                            ORDER BY i.SortOder, i.ImageId
                        ) pi
                        Where p.ProductName = @ProductName
                        ORDER BY p.ProductId;
                    ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar) { Value = productName });
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductItem
                {
                    id = reader.GetString(reader.GetOrdinal("Id")),
                    name = reader.GetString(reader.GetOrdinal("Name")),
                    price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    brand = reader.GetString(reader.GetOrdinal("Brand")),
                    category = reader.GetString(reader.GetOrdinal("Category")),
                    thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    createAt = reader.GetDateTime(reader.GetOrdinal("CreateAt"))
                });
            }
        }
        catch
        {
            throw;
        }
        return list;
    }
}