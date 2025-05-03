using ENT.Helpers;
using ENT.Models;
using ENT.Repository.Organization;
using Npgsql;
using Serilog;
namespace ENT.Repository.Account
{
    public class AccountRepository:IAccountRepository
    {
        private readonly string _connectionString;
        public AccountRepository(IConfiguration configuration)
        {
            this._connectionString = configuration.GetConnectionString("1st");
        }

        public async Task<Res<object>> RegisterUser(UserRegistration userRegistration)
        {
            try
            {
                if (userRegistration == null)
                {
                    return new Res<object>(500, "No Data Found To Insert");
                }

                await using var connection = new NpgsqlConnection(_connectionString);
                
                await connection.OpenAsync();

                var query = @"
                            SELECT * FROM public.user_registration_update(
                                @userid,
                                @fname,
                                @lname,
                                @email,
                                @password_hash,
                                @phone,
                                @org_id,
                                @role_id,
                                @profile_image_url,
                                @isactive,
                                @isverified,
                                @createdby
                            )";

                await using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@userid", userRegistration.Id);
                command.Parameters.AddWithValue("@fname", userRegistration.Fname);
                command.Parameters.AddWithValue("@lname", userRegistration.Lname);
                command.Parameters.AddWithValue("@email", userRegistration.Email);
                command.Parameters.AddWithValue("@password_hash", userRegistration.Password);
                command.Parameters.AddWithValue("@phone", userRegistration.PhoneNumber); 
                command.Parameters.AddWithValue("@profile_image_url", userRegistration.ProfileImageUrl);
                command.Parameters.AddWithValue("@isactive", userRegistration.IsActive);
                command.Parameters.AddWithValue("@isverified", userRegistration.IsVerified);

                

                // Pass Org Id After Creating Organization
                command.Parameters.AddWithValue("@org_id", userRegistration.OrgId);

                // Pass Role Id From Front =>  After Creating Organization => Default Role Automatic Generated so Pass Role that Name is 'Admin' => For that organization 
                command.Parameters.AddWithValue("@role_id", userRegistration.RoleId);

                // IF Auto Create User By Admin Then
                if (userRegistration.IsAuto)
                {
                    command.Parameters.AddWithValue("@createdby", userRegistration.CreatedBy);
                }
                else
                {
                    command.Parameters.AddWithValue("@createdby", DBNull.Value);
                }

                await using var reader = await command.ExecuteReaderAsync();

                // Expecting only one row, so we use 'if' instead of 'while'
                if (await reader.ReadAsync())
                {
                    var statusCode = reader.GetInt32(0);       // Column 1: status_code
                    var message = reader.GetString(1);         // Column 2: message
                    var userId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);  // Column 3: user_id

                    // You can return these as a custom object if needed

                    var result = new
                    {
                        StatusCode = statusCode,
                        Message = message,
                        UserId = userId
                    };

                    switch (statusCode)
                    {
                        case 1:
                            return new Res<object>(200, message, result);
                        default:
                            return new Res<object>(400, message, result);
                    }
                }

                return new Res<object>(404, "No response from DB function");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AccountRepository Error =>", userRegistration);
                return new Res<object>(500, $"Error: {ex.Message}");
            }
        }









    }
}
