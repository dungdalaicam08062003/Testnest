using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;


namespace WebsiteComputer.API.Admin
{
    [ApiController]
    [Route("api/admin/clients")]
    public class ClientAdmin : ControllerBase
    {
        private readonly IConfiguration _config;

        public ClientAdmin(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet]
        public async Task<IActionResult> GetAllClient()
        {

            //var connStr = ReturnConnStr();
        
            var listClient =  await  DBAdminClient.GetListClient(connStr) ;
            var json = JsonSerializer.Serialize(listClient, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(listClient);
        }
        [HttpGet("{clientID}")]
        public async Task<IActionResult> GetClientDetailAsync(int clientID)
        {
            //var connStr = ReturnConnStr();
            var clientDetail = await DBAdminClient.GetClientDetail(connStr, clientID);
            if (clientDetail is null) return NotFound();

            var json = JsonSerializer.Serialize(clientDetail, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(clientDetail);
        }
    }
   
}
