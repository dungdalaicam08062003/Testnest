
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Models;
using static WebsiteComputer.Models.ClientDtos;
namespace WebsiteComputer.Database
{
    internal class DBClient
    {

        //public static async Task Main(string[] args)
        //{
        //    var config = new ConfigurationBuilder()
        //   .SetBasePath(Directory.GetCurrentDirectory())
        //   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //   .Build();
        //    var connStr = config.GetConnectionString("Default")
        //        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        //    var clientNew = new ClientSignIn("d","d","d");
        //    await CreateClient(connStr, clientNew);


        //}
        public static async Task<List<ClientDetail>> readListClient(string connStr)
            => await GetClientList(connStr);
        public static async Task<List<ClientDetail>> GetClientList(string connStr)
        {
            var listClient = new List<ClientDetail>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                SELECT 
	                c.AccountID		  AS AccountID,
                    c.ClientCode      AS clientCode,
                    c.ClientName      AS clientName,
                    c.PhoneNumber     AS phoneNumber,
                    c.ClientAddress   AS clientAddress,
                    COALESCE(SUM(o.TotalPrice), 0) AS totalMoney
	
                FROM dbo.Client AS c
                LEFT JOIN dbo.[Orders] AS o
                    ON o.ClientID = c.ClientID
                GROUP BY 
	                c.AccountID,
                    c.ClientCode,
                    c.ClientName,
                    c.PhoneNumber,
                    c.ClientAddress;";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listClient.Add(new ClientDetail
                    {
                        accountID = reader.GetInt32(reader.GetOrdinal("AccountID")),
                        clientCode = reader.GetString(reader.GetOrdinal("clientCode")),
                        clientName = reader.GetString(reader.GetOrdinal("clientName")),
                        phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                        clientAddress = reader.GetString(reader.GetOrdinal("clientAddress")),
                        totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney"))
                    });
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return listClient;
        }
        public static async Task<GetClientProfile> GetClientProfile(string conStr, int clientID)
        {
            var clientProfile = new GetClientProfile();
            try 
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"SELECT 
                              [ClientName] as name 
	                          ,[username] as email
                          FROM [dbo].[Client] as cl
                          left join Account as a
                          on a.AccountID = cl.AccountID
                          where cl.ClientID = @clientID";
                using var cmd = new SqlCommand(sql , conn);
                cmd.Parameters.Add(new SqlParameter("@clientID", SqlDbType.Int) { Value = clientID });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    clientProfile.email = reader.GetString(reader.GetOrdinal("email"));
                    clientProfile.name = reader.GetString(reader.GetOrdinal("name"));

                }
            }
            catch
            {

            }
            return clientProfile;
        }
        public static async Task<bool> UpdateClientProfile(string conStr, ClientDtos.UpdateClientProfile client, int clientID)
        {
            try
            {
                using var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"begin try 
                            BEGIN TRAN;

                            UPDATE cl
                            SET 
                                cl.ClientName = @clientName
                            FROM dbo.Client cl
                            WHERE cl.ClientID = @clientID
                            UPDATE a
                            SET 
	                            a.Username = @gmail,
                                a.PasswordHash = @newPassword
                            from dbo.Account a     
                            inner JOIN Client cl       
                                ON cl.AccountID = a.AccountID
                            WHERE cl.ClientID = @clientID And a.PasswordHash = @oldPassword;

                                IF @@ROWCOUNT = 0
                                    THROW 50001, 'Invalid clientID or old password', 1;

                            COMMIT TRAN;
                            end try 
                            begin catch 
	                            rollback tran; 
	                            throw; 
                            end catch;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@clientID", SqlDbType.Int) { Value = clientID });
                cmd.Parameters.Add(new SqlParameter("@clientName", SqlDbType.NVarChar) { Value = client.name });
                cmd.Parameters.Add(new SqlParameter("@gmail", SqlDbType.VarChar) { Value = client.gmail });
                cmd.Parameters.Add(new SqlParameter("@oldPassword", SqlDbType.VarChar) { Value = client.currentPassword });
                cmd.Parameters.Add(new SqlParameter("@newPassword", SqlDbType.VarChar) { Value = client.newPassword });
                await cmd.ExecuteNonQueryAsync();

            }
            catch
            {
                return false;
            }
            return true;
        }
        public static async Task<int> CreateClient(string connStr, ClientSignIn clientSignIn)
        {
            int clientID = 0;
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                var now = DateTime.UtcNow;
                var ClientCode = $"CLI-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

                var AccCode = $"Acc-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                var sql = @"
                            DECLARE
    @AccountID INT,
    @ClientID INT;

DECLARE @AccountTable TABLE (AccountID INT);
DECLARE @ClientTable TABLE (ClientID INT);

INSERT INTO [dbo].[Account]
(
    AccountCode,
    Username,
    PasswordHash,
    Roles
)
OUTPUT INSERTED.AccountID INTO @AccountTable
VALUES
(
    @AccountCode,
    @Username,
    @PasswordHash,
    @Roles
);

SELECT @AccountID = AccountID FROM @AccountTable;

INSERT INTO [dbo].[Client]
(
    AccountID,
    ClientCode,
    ClientName,
    TotalMoney
)
OUTPUT INSERTED.ClientID INTO @ClientTable
VALUES
(
    @AccountID,
    @ClientCode,
    @ClientName,
    0.01
);

SELECT @ClientID = ClientID FROM @ClientTable;

INSERT INTO [dbo].[Cart] (ClientID)
VALUES (@ClientID);

SELECT 
    @AccountID AS AccountID,
    @ClientID AS ClientID;
                            ";
                using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
                cmd.Parameters.Add(new SqlParameter("@AccountCode", SqlDbType.VarChar) { Value = AccCode });
                cmd.Parameters.Add(new SqlParameter("@ClientCode", SqlDbType.VarChar) { Value = ClientCode });
                cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.VarChar) { Value = clientSignIn.email });
                cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarChar) { Value = clientSignIn.password });
                cmd.Parameters.Add(new SqlParameter("@ClientName", SqlDbType.NVarChar) { Value = clientSignIn.fullName });
                cmd.Parameters.Add(new SqlParameter("@Roles", SqlDbType.VarChar) { Value = "client" });
                var clientIDobj = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                clientID = Convert.ToInt32(clientIDobj);
                await tx.CommitAsync().ConfigureAwait(false);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return clientID;
        }
        public static async Task<int> UpdateClient(string connStr, string clientCode, string ClientName, string PhoneNumber )
        {
            int clientID = await ConnectDB.GetClientIDFromClientCode(connStr, clientCode);

            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                var sql = @"
                            UPDATE [dbo].[Client]
                               SET 
                                   [ClientName] = @ClientName
                                  ,[PhoneNumber] = @PhoneNumber
                             WHERE ClientCode = @ClientCode 
                                                        ";
                using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
                cmd.Parameters.Add(new SqlParameter("@ClientName", SqlDbType.NVarChar) { Value = ClientName });
                cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { Value = PhoneNumber });
                cmd.Parameters.Add(new SqlParameter("@ClientCode", SqlDbType.VarChar) { Value = clientCode });

                var clientIDobj = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                clientID = Convert.ToInt32(clientIDobj);
                await tx.CommitAsync().ConfigureAwait(false);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return clientID;
        }

        public static async Task deleteClient(string connStr, string clientCode)
        {
            int ClientID = await ConnectDB.GetClientIDFromClientCode(connStr, clientCode);
            try
            {

                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
               
                var sql = @"
                            BEGIN TRY
                                BEGIN TRAN;

                                -- Replace with your key or filter (ClientID or ClientCode)
                                DECLARE @ClientID INT ;
	                            select cl.AccountID from Client as cl where cl.ClientID = @ClientID

	                            delete Account where AccountID= ( select cl.AccountID from client as cl where ClientID = @ClientID)
                                -- 1) Delete dependent rows in child tables
                                DELETE FROM dbo.Cart WHERE ClientID = @ClientID;

                                -- If you also have Orders -> OrderItems, handle in correct order:
                                DELETE oi
                                FROM dbo.OrderItems oi
                                JOIN dbo.Orders o ON o.OrderID = oi.OrderID
                                WHERE o.ClientID = @ClientID;

                                DELETE FROM dbo.Orders WHERE ClientID = @ClientID;

                                -- 2) Delete the client
                                DELETE FROM dbo.Client WHERE ClientID = @ClientID;
	                            DELETE FROM [dbo].[Cart]
                                  WHERE Cart.ClientID = @ClientID
                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                THROW;
                            END CATCH;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = ClientID });
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("dddddd");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        public static async Task<ClientInformation?> Login(string conStr, ClientLogin clientLogin ) {
            var client = new ClientInformation();
            try {
                var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT [ClientCode] as clientCode
                                  ,[ClientName] as clientName
                                  ,[PhoneNumber] as phoneNumber
                                  ,[ClientAddress] as clientAddress 
                                  ,[TotalMoney] as totalMoney
	  
                              FROM [dbo].[Client] as c
  
                              left join dbo.Account as a
                              on a.AccountID = c.AccountID
                              where a.Username = @username and a.PasswordHash = @password
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar) { Value = clientLogin.email });
                cmd.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar) { Value = clientLogin.password });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    client.clientName = reader.GetString(reader.GetOrdinal("clientName"));
                    client.clientID = reader.GetString(reader.GetOrdinal("clientCode"));
                    client.phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber"));
                    client.clientAddress = reader.GetString(reader.GetOrdinal("clientAddress"));
                    client.totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney"));

                }
            }
            catch
            {
                throw;
            }
            if (client.clientName == null)
            {
                client = null;
            }
            return client;
        }
    }
}
