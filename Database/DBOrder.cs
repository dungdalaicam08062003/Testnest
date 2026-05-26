using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using WebsiteComputer.Models;
using static WebsiteComputer.Models.AdminOrder;


namespace WebsiteComputer.Database
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
        //    //OrderItemRequest orderItemRequest = new OrderItemRequest("P001", 2);
        //    //OrderItemRequest orderItemRequest2 = new OrderItemRequest("P001", 2);
        //    //List<OrderItemRequest> a = new List<OrderItemRequest>();
        //    //a.Add(orderItemRequest);
        //    //a.Add(orderItemRequest2);
        //    //CreateOrderRequest createOrderRequest = new CreateOrderRequest("CLI-0003", "Da nang", "0987383939", a);
        //    //var i = await addProductOrderByCart(connStr, createOrderRequest);
        //    var a = await GetListOrderItem(connStr);
        //    var json = JsonSerializer.Serialize(a, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.OutputEncoding = System.Text.Encoding.UTF8;
            
        //    Console.WriteLine(json);

        //}
        public static async Task<int?> addProductOrderByCart(string connStr, CreateOrderRequest req)
        {
            int? clientID = await ConnectDB.GetClientIDFromClientCode(connStr, req.ClientCode);
            int? orderID = null;
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                var now = DateTime.UtcNow;
                var orderCode = $"ORD-{now:yyyyMMdd}-{Random.Shared.Next(1000,9999)}";

                var insertOrder = new SqlCommand(
                    @"INSERT INTO dbo.Orders (ClientID, OrderCode, TotalPrice, Address, PhoneNumber, StatusOrders, CreateAt)
                    OUTPUT INSERTED.OrderID
                    VALUES (@ClientID, @OrderCode, 1.0, @Address, @PhoneNumber, N'pending', SYSUTCDATETIME());",    
                    conn, (SqlTransaction)tx);
                insertOrder.Parameters.AddWithValue("@ClientID", clientID);
                insertOrder.Parameters.AddWithValue("@OrderCode", orderCode);
                insertOrder.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value =
                        (object?)req.Address ?? DBNull.Value;
                insertOrder.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value =
                    (object?)req.PhoneNumber ?? DBNull.Value;

                object? scalar = await insertOrder.ExecuteScalarAsync();
                if (scalar != null && scalar != DBNull.Value)
                    orderID = Convert.ToInt32(scalar);
                foreach (var it in req.orderItem)
                {
                    int? productID = await ConnectDB.GetProductIDFromProductCode(connStr, it.ProductCode);
                    var cmd = new SqlCommand(@"
                        INSERT INTO dbo.OrderItems (OrderID, ProductID, Quantity, Price)
                        SELECT
                            @OrderID,
                            p.ProductID,
                            @Quantity,
                            p.Price
                        FROM dbo.Products AS p
                        WHERE p.ProductID = @ProductID;

                        -- 2) Cập nhật tổng tiền của đơn (nếu TotalPrice là cột thường, KHÔNG dùng nếu là computed)
                        UPDATE o
                        SET o.TotalPrice = x.SumTotal
                        FROM dbo.Orders AS o
                        CROSS APPLY (
                            SELECT SUM(oi.Quantity * oi.Price) AS SumTotal
                            FROM dbo.OrderItems AS oi
                            WHERE oi.OrderID = @OrderID
                        ) AS x
                        update c 
                        set c.TotalMoney = coalesce(s.SumTotal,0)
                        from dbo.Client as c
                        left join(
                        select o.ClientID, SUM(o.TotalPrice) as SumToTal
                        from dbo.Orders as o
                        group by o.ClientID

                        ) as s 
                        on s.ClientID = c.ClientID

                        UPDATE oi
                        SET oi.PriceAlterDiscount =
                            CASE
                                WHEN p.DiscountID IS NULL THEN p.Price*oi.Quantity
                                ELSE p.Price * (1 - d.DiscountValue)*oi.Quantity
                            END
                        FROM OrderItems oi
                        JOIN Products p
                            ON oi.ProductID = p.ProductID
                        LEFT JOIN Discount d
                            ON p.DiscountID = d.DiscountID
                        where p.productID = @ProductID;

                        UPDATE o
                        SET o.FinalPrice = ISNULL(x.TotalPrice*(1-d.DiscountValue), 0)
                        FROM Orders o
                        LEFT JOIN Discount d
                            ON d.DiscountID = o.DiscountID
                        OUTER APPLY (
                            SELECT SUM(oi.PriceAlterDiscount) AS TotalPrice
                            FROM OrderItems oi
                            WHERE oi.OrderID = o.OrderID
                        ) x
                        where o.OrderID = @OrderID"";
                        ", conn, (SqlTransaction)tx);
                    
                    cmd.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderID;
                    cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = productID;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value =it.Quantity;
                    

                    await cmd.ExecuteNonQueryAsync();
                }
                await tx.CommitAsync();
            }
            catch
            {
                throw;
            }
            return orderID;
        }
        public static async Task<int?> addproductOrder(string connStr,OrderRequest dto)
        {
            int? clientId = await ConnectDB.GetClientIDFromClientCode(connStr, dto.clientCode);
            if (clientId is null) throw new InvalidOperationException("ClientCode is null .");

            int productId = await ConnectDB.GetProductIDFromProductCode(connStr, dto.productCode);

            int? orderID = null;

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
                    VALUES (@ClientID, @OrderCode, 1.0, @Address, @PhoneNumber, N'pending', SYSUTCDATETIME());

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
                cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = dto.quantity;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value =
                    (object?)dto.address ?? DBNull.Value;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value =
                    (object?)dto.phoneNumber ?? DBNull.Value;

                // 4) Lấy OrderID
                object? scalar = await cmd.ExecuteScalarAsync();
                if (scalar != null && scalar != DBNull.Value)
                    orderID = Convert.ToInt32(scalar);
            }
            catch (Exception e)
            {
                // TODO: log e
                Console.WriteLine(e.Message);
            }

            return orderID;
        }
        public static async Task<OrderDetail> GetOrderDetail(string conStr, string orderCodeDetail)
        {
            var orderId = await ConnectDB.GetOrderIDFromOrderCode(conStr, orderCodeDetail);
            OrderDetail? orderDetail = null;
            var listOrder = new List<OrderItems>();
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT 
                                o.[TotalPrice] as totalPrice
                                ,o.[CreateAt] as createAt
                                ,[Address] as addressOrder
                                ,o.[PhoneNumber] as phoneNumber
                                ,o.TotalPrice as totalMoney
	                            ,o.FinalPrice as finalPrice
                                ,ClientName as clientName
                            FROM [dbo].[Orders] as o
                            left join Client as cl 
                            on o.ClientID = cl.ClientID 

                            where o.OrderID = @orderID

                            Select  
                                    oi.OrderID as orderID, 
                                    ProductName as productName
                                    ,Quantity  as quantity,
		                            oi.Price as price 
                                    ,oi.TotalPrice as orderItemTotalPrice,
                                    oi.PriceAlterdiscount as priceAfterDiscount 
                            from OrderItems as oi
                            left join Products as p 
                            on p.ProductID = oi.ProductID
                            left join Discount as di 
                            on p.DiscountID = di.DiscountID
                            where oi.OrderID = @orderID";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@orderID", SqlDbType.Int) { Value = orderId });
                var reader = await cmd.ExecuteReaderAsync();
                ;
                if (await reader.ReadAsync())
                {
                    orderDetail = new OrderDetail
                    {
                        order = new GetOrderList
                        {
                            orderID = orderCodeDetail,
                            clientName = reader.GetString(reader.GetOrdinal("clientName")),
                            phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                            Address = reader.GetString(reader.GetOrdinal("addressOrder")),
                            creatAt = reader.GetDateTime(reader.GetOrdinal("createAt")),
                            totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney")),
                            totalMoneyAfterDiscount = reader.GetDecimal(reader.GetOrdinal("finalPrice"))
                        }

                    };
                }
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    var testOrder = new OrderItems();
                    testOrder.totalPriceAfterDiscount = reader.GetDecimal(reader.GetOrdinal("priceAfterDiscount"));
                    listOrder.Add(new OrderItems
                    {
                        productName = reader.GetString(reader.GetOrdinal("productName")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                        totalPrice = reader.GetDecimal(reader.GetOrdinal("orderItemTotalPrice")),
                        totalPriceAfterDiscount = reader.GetDecimal(reader.GetOrdinal("priceAfterDiscount"))
                    });
                }
            }
            catch
            {
                throw;
            }
            if (orderDetail != null)
            {
                orderDetail.listOrderItem = listOrder;
            }
            return orderDetail;

        }
        public static async Task<List<Order>> GetListOrder(string connStr)
        {
            var list = new List<Order>();
            try
            {
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT [OrderID]
                          ,[OrderCode] as id
                          ,cl.[ClientCode] as clientCode
                          ,[TotalPrice] as totalprice
                          ,[StatusOrders] as statusOrder
                          ,[CreateAt]	as createAt
                          ,[Address]	as address
                          ,o.[PhoneNumber] as phoneNumber
                      FROM [dbo].[Orders] as o
                      left join dbo.Client as cl
                      on cl.ClientID = o.ClientID
                            ";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
             
                while (await reader.ReadAsync())
                {
                    list.Add(new Order
                    {
                        OrderCode = reader.GetString(reader.GetOrdinal("id")),
                        ClientCode = reader.GetString(reader.GetOrdinal("clientCode")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("totalprice")),
                        StatusOrders = reader.GetString(reader.GetOrdinal("statusOrder")),
                        CreateAt = reader.GetDateTime(reader.GetOrdinal("createAt")),
                        PhoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                        Address = reader.GetString(reader.GetOrdinal("address"))
                    });

                }
            }
            catch
            {
                throw;
            }
            return list;
        }
        public static async Task<List<OrderItem>> GetListOrderItem(string connStr)
        {
            var list = new List<OrderItem>();
            try
            {
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT
                            o.OrderCode as orderCode,
	                        cl.ClientName as clientName,
                            p.ProductName as ProductName,
                            ot.Quantity as quantity,
                            p.Price as price ,
                            ot.TotalPrice as totalPrice
                        FROM OrderItems ot
                        LEFT JOIN Products p 
                            ON p.ProductID = ot.ProductID
                        LEFT JOIN Orders o
                            ON o.OrderID = ot.OrderID
                        LEFT JOIN Client cl
                            ON cl.ClientID = o.ClientID;
                            ";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new OrderItem
                    {
                        OrderCode = reader.GetString(reader.GetOrdinal("orderCode")),
                        ClientName = reader.GetString(reader.GetOrdinal("clientName")),
                        ProductName   = reader.GetString(reader.GetOrdinal("ProductName")),
                        price  = reader.GetDecimal(reader.GetOrdinal("price")),
                        quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                        totalPrice = reader.GetDecimal(reader.GetOrdinal("totalPrice"))
                    });

                }
            }
            catch 
            {
                throw;
            }
            return list;
        }
        public static async Task<int?> updateStatusOrder(string connStr, string orderCode, string statusOrder)
        {
            int orderID = await ConnectDB.GetOrderIDFromOrderCode(connStr, orderCode);
            try
            {
                using var conn =  ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"UPDATE [dbo].[Orders]
                           SET 
                           --'pending','processing','shipping','completed','cancelled'
                              [StatusOrders] = @StatusOrders
                         WHERE OrderCode = @OrderCode
                        GO";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@statusOrders", SqlDbType.VarChar) { Value = statusOrder });
                cmd.Parameters.Add(new SqlParameter("@orderCode", SqlDbType.VarChar) { Value = orderCode });
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return orderID;
        }
        public static async Task<int?> updateInfomationOrder(string connStr, string orderCode, string phoneNumber, string address)
        {
            int orderID = await ConnectDB.GetOrderIDFromOrderCode(connStr, orderCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                       Begin try
	                        Begin tran 
	                        UPDATE [dbo].[Orders]
	                           SET 
	                           --'pending','processing','shipping','completed','cancelled'
			                        [Address] = 'a'
		                          ,[PhoneNumber] = '99999999'
	                         WHERE OrderCode = 'ORD-20260316-8606'

	                        Commit tran
                        End try
                        begin catch
	                        if @@TRANCOUNT > 0 rollback tran
	                        thow
                        end catch ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderCode", SqlDbType.VarChar) { Value = orderCode });
                cmd.Parameters.Add(new SqlParameter("@Phonenumber", SqlDbType.VarChar) { Value = phoneNumber });
                cmd.Parameters.Add(new SqlParameter("@Address", SqlDbType.VarChar) { Value = address });
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return orderID;
        }
        public static async Task deleteOrder(string connStr, string orderCode)
        {
            
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            Declare 
	                            --@OrderCode varchar(100) = 'ORD-20260317-6213',
	                            @ClientID INT;
                            select @ClientID = ClientID
                            From orders
                            where OrderCode = @OrderCode
                            Begin transaction
                            DELETE FROM [dbo].[Orders]
                                  WHERE OrderCode = @OrderCode


                            UPDATE client
                            SET TotalMoney = ISNULL((
	                            select sum(totalPrice)
	                            from orders
	                            where ClientID = @ClientID
                            ),0)
                            where ClientID = @ClientID
                            commit transaction
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@OrderCode", SqlDbType.VarChar) { Value = orderCode });
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
       
        }
    }
}
