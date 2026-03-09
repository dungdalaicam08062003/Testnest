using ConsoleApp1.Models;
using Microsoft.Data.SqlClient;
using System.Text.Encodings.Web;
using System.Text.Json;
using ConsoleApp1.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Websitecomputer.Models;
using System.Net;
using Websitecomputer.DB;
public static class DBHomepage
{
    // Trả về LIST thay vì string
    public static async Task<List<ProductListItem>> RunAllAsync(string connStr)
        => await SelectAllProductsAsList(connStr);

    public static async Task<List<ProductListItem>> SelectAllProductsAsList(string connStr)
    {
        var list = new List<ProductListItem>();
        try
        {
            using var conn = ConnectDB.Create(connStr);
            await conn.OpenAsync();

            var sql = @"
                SELECT
                    p.ProductCode  AS Id,
                    p.ProductName  AS Name,
                    p.Price        AS Price,
                    b.BrandCode    AS Brand,
                    pi.ImageUrl    AS Thumbnail,
                    p.Stock        AS Stock,
                    p.CreateAt     AS CreateAt
                FROM dbo.Products AS p
                LEFT JOIN dbo.Brands AS b
                       ON b.BrandId = p.BrandId
                OUTER APPLY (
                    SELECT TOP (1) i.ImageUrl
                    FROM dbo.ProductImages AS i
                    WHERE i.ProductId = p.ProductId
                    ORDER BY i.SortOder ASC, i.ImageId ASC
                ) AS pi
                ORDER BY p.ProductId;";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ProductListItem
                {
                    Id = reader.GetString(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Brand = reader.GetString(reader.GetOrdinal("Brand")),
                    Thumbnail = reader.IsDBNull(reader.GetOrdinal("Thumbnail"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Thumbnail")),
                    Stock = reader.IsDBNull(reader.GetOrdinal("Stock"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("Stock")),
                    CreateAt = reader.GetDateTime(reader.GetOrdinal("CreateAt"))
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Tùy bạn: có thể throw ra để endpoint trả 500
        }
        return list;
    }
}