using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using  WebsiteComputer.Models;
using static WebsiteComputer.Database.DBClient;
namespace API.ClientInterface
{
    [ApiController]
    [Route("api/signup")]
    public class SignUp : ControllerBase
    {
        private readonly IConfiguration _config;

        public SignUp(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing Supabase connection string");
        [HttpPost]
        public async Task<IActionResult> CreateNewClient([FromBody] ClientDtos.ClientSignIn client)
        {

            try
            {
                var clientID = await CreateClient(connStr, client);
                return Ok(clientID);
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }

        }
    }
}
