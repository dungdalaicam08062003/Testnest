using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using WebsiteComputer.Database.DBAdmin;
using WebsiteComputer.Models;
namespace API.Admin
{ 
    [ApiController]
    [Route("api/admin/products")]
    public class ProductAdmin : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductAdmin(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {

            var listProduct = await DBProduct.ProductGetList(connStr);
            var json = JsonSerializer.Serialize(listProduct, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(listProduct);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody]CreateProductRequest req)
        {
            try {
                var productCode = await DBProduct.createProduct(connStr, req.ProductInfo , req.ProductSpecs);
                return Ok(new
                {
                    message = "Create product success",
                    productCode
                });
            }
            catch (Exception ex)
            { 
                return BadRequest(ex.Message); 
            }
            
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(string productCode)
        {
            try
            {
                var productID = await DBProductDetail.deleteProductDetail(connStr, productCode);
                return Ok(new
                {
                    message = "Deleted product success",
                    productCode
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
