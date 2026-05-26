using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;
using WebsiteComputer.Models.Policy;

namespace API.Admin
{
    [ApiController]
    [Route("api/Admin/Discount")]
    public class DiscountAdmin : ControllerBase
    {
        private readonly IConfiguration _config;

        public DiscountAdmin(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetDiscount(string Id)
        {
            var discountInfo = await DBAdminDiscount.ReadDiscount(connStr, Id);
            var json = JsonSerializer.Serialize(discountInfo, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(discountInfo);
        }
        [HttpGet]
        public async Task<IActionResult> getDiscountList()
        {
            var discount = await DBAdminDiscount.ReadListDiscount(connStr);
            return Ok(discount);
        }
        [HttpPost]
        public async Task<IActionResult> postDiscount([FromBody]Discount.CreateDiscountPolicy discountPolicyInfo)
        {
            var newDiscount = await DBAdminDiscount.CreateDiscount(connStr, discountPolicyInfo);
            return Ok(newDiscount);
        }
        [HttpPut]
        public async Task<IActionResult> putDiscount([FromBody] Discount.UpdateDiscountPolicy discountPolicyInfo)
        {
            var newDiscountUpdate = await DBAdminDiscount.UpdateDiscount(connStr, discountPolicyInfo);
            return Ok(newDiscountUpdate);
        }
        //[HttpPut("")]
        //public async Task
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteDiscount(string id)
        {
            var delete = await DBAdminDiscount.DeleteDiscount(connStr, id);
            return Ok(delete);
        }

    }
}
