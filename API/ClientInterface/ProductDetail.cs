using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
namespace API.ClientInterface
{
    [ApiController]
    [Route("api/productDetail")]

    public class ProductDetail : ControllerBase
    {
        private readonly IConfiguration _config;
        public ProductDetail(IConfiguration config)
        {
            _config = config;
        }
        private string connStr =>
            _config.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing Supabase connection string");

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await DBProductDetail.ReadAsDtoAsync(connStr, id);

            return Ok(product);
        }
    }
}