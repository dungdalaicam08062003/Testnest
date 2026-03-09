using ConsoleApp1.DB;
using Websitecomputer.DB;
using ConsoleApp1.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using Websitecomputer.Page;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Websitecomputer.DB
{
    public static class DBProductDetail
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
        //    var json = await ReadAsJsonAync(connStr, "P001");
        //    Console.WriteLine(json);

        //}

        public static async Task<ProductSpec?> GetProductSpecAsync(string connStr, string ProductCode)
        {
            ProductSpec? spec = null;
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();

                var sql = @$"                  
                            SELECT
                                MAX(CASE WHEN SpecKey = 'CPU'     THEN SpecValue END) AS CPU,
                                MAX(CASE WHEN SpecKey = 'RAM'     THEN SpecValue END) AS RAM,
                                MAX(CASE WHEN SpecKey = 'Storage' THEN SpecValue END) AS Storage,
                                MAX(CASE WHEN SpecKey = 'Display' THEN SpecValue END) AS Display,
                                MAX(CASE WHEN SpecKey = 'GPU'     THEN SpecValue END) AS GPU
                            FROM dbo.ProductSpecs
                            WHERE ProductID = @ProductID
                                                        ";
                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = ProductID });

                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string? GetSafe(string col)
                        => reader.IsDBNull(reader.GetOrdinal(col)) ? null : reader.GetString(reader.GetOrdinal(col));
                    spec = new ProductSpec
                    {
                        CPU = GetSafe("CPU") ?? string.Empty,
                        Storage = GetSafe("Storage") ?? string.Empty,
                        RAM = GetSafe("RAM") ?? string.Empty,
                        Display = GetSafe("Display") ?? string.Empty,
                        GPU = GetSafe("GPU") ?? string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return spec;
        }
        public static async Task<ProductMainInfo?> GetProductMainAsync(string connStr, string ProductCode)
        {
            ProductMainInfo? info = null;
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @$"
                            SELECT
                                p.ProductName  AS Name,
                                p.Price        AS Price,
                                p.Stock        AS Stock,
                                b.BrandName    AS Brand,
                                (
                                    SELECT TOP (1) i.ImageUrl
                                    FROM dbo.ProductImages AS i
                                    WHERE i.ProductId = p.ProductId
                                    ORDER BY i.SortOder ASC, i.ImageId ASC
                                ) AS Thumbnail
                            FROM dbo.Products AS p
                            LEFT JOIN dbo.Brands AS b ON b.BrandId = p.BrandId
                            WHERE p.ProductID = @ProductID";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = ProductID });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {

                    string name = reader.GetString(reader.GetOrdinal("Name"));
                    decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));
                    int stock = reader.GetInt32(reader.GetOrdinal("Stock"));
                    string brand = reader.IsDBNull(reader.GetOrdinal("Brand")) ? string.Empty : reader.GetString(reader.GetOrdinal("Brand"));
                    string? thumb = reader.IsDBNull(reader.GetOrdinal("Thumbnail")) ? null : reader.GetString(reader.GetOrdinal("Thumbnail"));

                    info = new ProductMainInfo
                    {
                        Name = name,
                        Price = price,
                        Stock = stock,
                        Brand = brand,
                        Thumbnail = thumb
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
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();

                var sql = @"Select i.ImageUrl
                            From dbo.ProductImages AS i
                            where i.ProductID = @ProductID and i.SortOder > 1
                            Order BY i.SortOder, i.ImageID;
                            ";
                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = ProductID });

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
        //return object
        public static async Task<ProductDetail?> ReadAsDtoAsync(string connStr, string ProductCode)
        {
            var main = await GetProductMainAsync(connStr, ProductCode);
            if (main is null) return null;

            var specs = await GetProductSpecAsync(connStr, ProductCode);
            var images = await GetImageAsync(connStr, ProductCode);

            return new ProductDetail
            {
                ProductId = ProductCode,
                Name = main.Name,
                Price = main.Price,
                Stock = main.Stock,
                Brand = main.Brand,
                Thumbnail = main.Thumbnail,
                Specs = specs,
                Images = images
            };
        }
        // return string Json
        public static async Task<string> ReadAsJsonAync(string connStr, string ProductCode)
        {
            int productID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            var main = await GetProductMainAsync(connStr, ProductCode);
            if (main is null)
            {
                return JsonSerializer.Serialize(new { productID, message = "product not found" });
            }
            var specs = await GetProductSpecAsync(connStr, ProductCode);
            var images = await GetImageAsync(connStr, ProductCode);
            var dto = new ProductDetail
            {
                ProductId = ProductCode,
                Name = main.Name,
                Price = main.Price,
                Stock = main.Stock,
                Brand = main.Brand,
                Thumbnail = main.Thumbnail,
                Specs = specs,
                Images = images
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            return json;
        }
        
    }
}

