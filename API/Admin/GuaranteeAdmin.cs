using Database.DBAdmin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;
using WebsiteComputer.Models.Policy;

namespace API.Admin
{
    [ApiController]
    [Route("api/Admin/Guarantee")]
    public class GuaranteeAdmin : ControllerBase
    {
        private readonly IConfiguration _config;

        public GuaranteeAdmin(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuarantee(string id)
        {
            var results = await DBAdminGuarantee.ReadGuarantee(connStr, id);
            return Ok(results);
        }
        [HttpGet]
        public async Task<IActionResult> GetGuaranteeList()
        {
            var results = await DBAdminGuarantee.ReadListGuarantee(connStr);
            return Ok(results);            
        }
        [HttpPost]
        public async Task<IActionResult> PostGuarantee([FromBody]Guarantee.GuaranteeProduct guarantee)
        {
            var results = await DBAdminGuarantee.CreateGuarantee(connStr, guarantee);
            return Ok(results);  
        }
        [HttpPut]
        public async Task<IActionResult> PutGuarantee([FromBody]Guarantee.GuaranteeProduct guarantee)
        {
            var results = await DBAdminGuarantee.UpdateGuarantee(connStr, guarantee);
            return Ok(results);             
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuarantee(string id)
        {
            var results = await DBAdminGuarantee.DeleteGuarantee(connStr, id);
            return Ok(results);
        }

    }
}
