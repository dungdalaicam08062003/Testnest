// using Npgsql;
// using WebsiteComputer.Database;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using System.Net;
// using System.Text.Encodings.Web;
// using System.Text.Json;
// using System.Text.Unicode;
// using WebsiteComputer.Models;

// namespace WebsiteComputer
// {
//     public class TestSupabase{
//         public static async Task Main(string[] args)
//         {
//            // var connStr ="Host=db.iztvapuoljhjhiebgtll.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=duonganhdung123456789;SSL Mode=Require;Trust Server Certificate=true;";
//             var config = new ConfigurationBuilder()
//                 .SetBasePath(Directory.GetCurrentDirectory())
//                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                 .Build();
//             var connStr = config.GetConnectionString("Supabase")
//                 ?? throw new InvalidOperationException("Missing ConnectionStrings:Supabase");
//             await using var conn = new NpgsqlConnection(connStr);
//             await conn.OpenAsync();
//             Console.WriteLine("Connected to Supabase ✅");
// //             using var conn = new NpgsqlConnection(connStr);

// //             await conn.OpenAsync();


//             using var cmd = new NpgsqlCommand("select * from products", conn);

//             using var reader = await cmd.ExecuteReaderAsync();
//             while (reader.Read())
//             {
//                 Console.WriteLine(reader["product_name"]);
//             }
//         }
//     }
// }

