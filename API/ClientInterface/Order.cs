using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
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
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
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
