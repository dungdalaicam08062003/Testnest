using ConsoleApp1.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Websitecomputer.DB;
using Websitecomputer.Models;


var builder = WebApplication.CreateBuilder(args);

// take server database
var config = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
   .Build();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.WriteIndented = true;          // đẹp khi dev; prod thường để false
    o.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

var connStr = config.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
//Read origin for CORs List
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("CorsPolicy");

var ListProducts = await DBHomepage.RunAllAsync(connStr);

app.MapGet("/api/products", () => Results.Ok(ListProducts));

app.MapGet("/api/products/{ClientID}", async (string ClientID, IConfiguration cfg) =>
{
    var ProductDetail = await DBProductDetail.ReadAsDtoAsync(connStr, ClientID);
    if (ProductDetail is null) return Results.NotFound(); 
    return Results.Ok(ProductDetail);
});
//CLI-0001
app.MapGet("/api/cart/{ClientID}", async (string ClientID, IConfiguration cfg) =>
{
    var Cart = await DBCart.ReadAsJsonAsync(connStr, ClientID);
    return Cart is null ? Results.NotFound() : Results.Ok(Cart);
});


app.MapPost("/api/orders", async (OrderRequest order, IConfiguration cfg) =>
{
    // Validate cơ bản (có thể dùng DataAnnotations + [ApiController] nếu dùng Controllers)
    if (string.IsNullOrWhiteSpace(order.ClientCode))
        return Results.BadRequest("ClientCode là bắt buộc.");
    if (string.IsNullOrWhiteSpace(order.ProductCode))
        return Results.BadRequest("ProductCode là bắt buộc.");
    if (order.Quantity <= 0)
        return Results.BadRequest("Quantity phải > 0.");

    var connStr = cfg.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connStr))
        return Results.Problem("Thiếu connection string.", statusCode: 500);

    try
    {
        var orderId = await DBOrder.addproductOrder(
            connStr!,
            order.ClientCode,
            order.ProductCode,
            order.Quantity,
            order.Address,
            order.PhoneNumber
        );

        return Results.Created($"/api/orders/{orderId}", new { orderId });
    }
    catch (Exception ex)
    {
        // TODO: log ex
        return Results.Problem(title: "Không tạo được đơn hàng.", statusCode: 500, detail: ex.Message);
    }
});

await app.RunAsync();


