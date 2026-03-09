using ConsoleApp1.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Websitecomputer.Models;
using Websitecomputer.Page;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Websitecomputer.Models.ClientDtos;

namespace Websitecomputer.DB
{
    internal class DBOrder
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
            
        //    var i = await addproductOrder(connStr,  "CLI-0001", "P001", 5,"da nang","0956646123");
        //    Console.WriteLine(i);

        //}
        public static async Task<int?> addproductOrder(string connStr,string clientCode,string productCode,int quantity,string address,string phoneNumber)
        {
            int? clientId = await ConnectDB.GetClientIDFromClientCode(connStr, clientCode);
            if (clientId is null) throw new InvalidOperationException("ClientCode không tồn tại.");

            int productId = await ConnectDB.GetProductIDFromProductCode(connStr, productCode);

            int? orderId = null;

            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();

                var sql = @"
                BEGIN TRY
                    BEGIN TRAN;

                    -- Lấy giá từ Products
                    DECLARE @Price DECIMAL(18,2);
                    SELECT @Price = p.Price
                    FROM dbo.Products AS p
                    WHERE p.ProductID = @ProductID;

                    IF @Price IS NULL
                        THROW 50001, N'ProductID không tồn tại', 1;

                    -- Sinh mã đơn
                    DECLARE @OrderCode NVARCHAR(50) =
                        N'ORD-' + CONVERT(CHAR(8), GETDATE(), 112) + N'-' +
                        REPLACE(CONVERT(CHAR(8), GETDATE(), 108), ':', '') + N'-' +
                        RIGHT(CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(10)), 4);

                    -- Insert Orders (TotalPrice tạm 0, sẽ cập nhật sau)
                    INSERT INTO dbo.Orders (ClientID, OrderCode, TotalPrice, Address, PhoneNumber, StatusOrders, CreateAt)
                    VALUES (@ClientID, @OrderCode, 1.0, @Address, @PhoneNumber, N'preparing', SYSUTCDATETIME());

                    DECLARE @OrderID INT = SCOPE_IDENTITY();

                    -- Insert OrderItems (không chèn TotalPrice nếu là computed)
                    INSERT INTO dbo.OrderItems (OrderID, ProductID, Quantity, Price)
                    VALUES (@OrderID, @ProductID, @Quantity, @Price);

                    -- Cập nhật tổng tiền order
                    UPDATE o
                    SET o.TotalPrice = x.SumTotal
                    FROM dbo.Orders AS o
                    CROSS APPLY (
                        SELECT SUM(oi.Price * oi.Quantity) AS SumTotal
                        FROM dbo.OrderItems AS oi
                        WHERE oi.OrderID = o.OrderID
                    ) AS x
                    WHERE o.OrderID = @OrderID;

                    COMMIT TRAN;

                    -- Trả về OrderID (cột đầu tiên dùng cho ExecuteScalar)
                    SELECT @OrderID;
                END TRY
                BEGIN CATCH
                    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                    THROW;
                END CATCH;";

                using var cmd = new SqlCommand(sql, conn);

                // 3) Mapping tham số đúng kiểu + độ dài
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId.Value;
                cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value =
                    (object?)address ?? DBNull.Value;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value =
                    (object?)phoneNumber ?? DBNull.Value;

                // 4) Lấy OrderID
                object? scalar = await cmd.ExecuteScalarAsync();
                if (scalar != null && scalar != DBNull.Value)
                    orderId = Convert.ToInt32(scalar);
            }
            catch (Exception e)
            {
                // TODO: log e
                Console.WriteLine(e.Message);
            }

            return orderId;
        }
    }
}
