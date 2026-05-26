using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
using WebsiteComputer.Models;

namespace API.ClientInterface
{
    [ApiController]
    [Route("api/order")]
    public class Order : ControllerBase
    {
        private readonly IConfiguration _config;
        public Order(IConfiguration config)
        {
            _config = config;
        }
        private string connStr =>
            _config.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing Supabase connection string");
        [HttpGet("{orderID}")]
        public async Task<IActionResult> getOrderDetail(string orderID)
        {
            var orderDetail = await DBOrder.GetOrderDetail(connStr, orderID);
            var json = JsonSerializer.Serialize(orderDetail, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(orderDetail);
        }
        [HttpPost]
        public async Task<IActionResult> CreateNewOrder(CreateOrderRequest order)
        {
           
            var orderId = await DBOrder.addProductOrderByCart(
                    connStr,
                    order
                    );
            return Ok(orderId);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateOrder(UpdateOrder order)
        {

            var orderId = await DBOrder.updateInfomationOrder(
                    connStr,
                    order.orderCode,
                    order.phoneNumber,
                    order.address
                    );
            return Ok(orderId);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteOrder(UpdateOrder order)
        {

            await DBOrder.deleteOrder(
                    connStr,
                    order.orderCode
                    );
            return Ok("deleted Order");
        }
    }
}
