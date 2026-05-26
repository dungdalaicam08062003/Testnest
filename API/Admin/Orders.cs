using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;
using static WebsiteComputer.Database.DBOrder;
using WebsiteComputer.Models;
namespace WebsiteComputer.API.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    public class Orders : ControllerBase
    {
        private readonly IConfiguration _config;

        public Orders(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        [HttpGet]
        public async Task<IActionResult> GetListOrderAdmin()
        {
            var listOrder = await DBOrder.GetOrderList(connStr);
            var json = JsonSerializer.Serialize(listOrder, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(listOrder);
        }
        [HttpGet("{orderCode}")]
        public async Task<IActionResult> getOrderDetail(string clientCode)
        {
            var orderDetail = await DBOrder.GetOrderDetail(connStr, clientCode);
            var json = JsonSerializer.Serialize(orderDetail, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(orderDetail);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateOrder([FromBody]UpdateOrderStatus order)
        {

            var orderId = await updateStatusOrder(
                    connStr,
                    order.orderCode,
                    order.status
                    );
            return Ok(orderId);
        }
    }
}
