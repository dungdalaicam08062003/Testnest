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
    [Route("api/login")]
    public class Login : ControllerBase
    {
        private readonly IConfiguration _config;

        public Login(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing Supabase connection string");
        [HttpPost("check")]
        public async Task<IActionResult> LoginAction([FromBody] ClientDtos.ClientLogin client)
        {

            try
            {
                var clientID = await DBClient.Login(connStr, client);
                if (clientID == null) {
                    return BadRequest("Invalid username or password");
                }
                return Ok(clientID);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }
    }
}
