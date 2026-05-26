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
    [Route("api/client")]
    public class Client : ControllerBase
    {
        private readonly IConfiguration _config;

        public Client(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Supabase")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing Supabase connection string");

        [HttpGet("{clientID}")]
        public async Task<IActionResult> GetClientProfile(int clientID)
        {
            var clientProfile = await DBClient.GetClientProfile(connStr, clientID);
            return Ok(clientProfile);
        }
        [HttpPut("{clientID}")]
        public async Task<IActionResult> PutClientProfile([FromBody]ClientDtos.UpdateClientProfile client , int clientID)
        {
            var clientProfile = await DBClient.UpdateClientProfile(connStr, client, clientID);
            return Ok(clientProfile);
        }
    }
}
