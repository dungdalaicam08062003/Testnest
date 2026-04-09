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
    [Route("api/products")]
    
    public class HomePage : ControllerBase
    {
        private string connStr =>
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION") ??
            throw new InvalidOperationException("Missing database connection");

        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var listProduct = await DBHomepage.SelectAllProductsAsList(connStr);
            
            return Ok(listProduct);
        }
        [HttpGet("category/{CategoryName}")]
        public async Task<IActionResult> GetProductByCategory(string CategoryName)
        {
            var listProduct = await DBHomepage.SelectAllProductsByCategory(connStr, CategoryName);
            return Ok(listProduct);
        }
        [HttpGet("brand/{BrandName}")]
        public async Task<IActionResult> GetProductByBrand(string BrandName)
        {
            var listProduct = await DBHomepage.SelectAllProductsByBrand(connStr, BrandName);
            return Ok(listProduct);
        }
        [HttpGet("search/{productName}")]
        public async Task<IActionResult> GetProductBySearchbar(string productName)
        {
            var listProduct = await DBHomepage.SelectAllProductsByProductName(connStr, productName);
            return Ok(listProduct);
        }
    }
}
