using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;

namespace API.Admin
{
    [ApiController]
    [Route("api/admin/Dashboard")]
    public class DashboardInteface: ControllerBase
    {
        private readonly IConfiguration _config;

        public DashboardInteface(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        [HttpGet]
        public async Task<IActionResult> DisplayDashboard()
        {
            var displayDashboard = await Dashboard.GetDashboard(connStr);
            var json = JsonSerializer.Serialize(displayDashboard, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            Console.WriteLine(json);
            return Ok(displayDashboard);
        }

    }
}
