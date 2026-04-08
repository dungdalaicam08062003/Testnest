using WebsiteComputer.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WebsiteComputer.Models;



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
    //o.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

var connStr = config.GetConnectionString("Supabase")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Supabase");

// //Read origin for CORs List
// var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

// builder.Services.AddCors(opt =>
// {
//     opt.AddPolicy("CorsPolicy", p =>
//         p.WithOrigins(allowedOrigins)
//          .AllowAnyHeader()
//          .AllowAnyMethod());
// });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ✅ Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// ✅ Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.MapControllers();
//http://localhost:5000/swagger

await app.RunAsync();


