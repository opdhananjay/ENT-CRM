using ENT.Helpers;
using ENT.Models;
using ENT.Repository.Organization;
using ENT.Services.JWT;
using Npgsql;
using Serilog;
using System.Data;
using System.Security.Cryptography;
namespace ENT.Repository.Account
{
    public class AccountRepository:IAccountRepository
    {
        private readonly string _connectionString;
        private readonly ITokenService _tokenService;
        public AccountRepository(IConfiguration configuration, ITokenService tokenService)
        {
            this._connectionString = configuration.GetConnectionString("1st");
            this._tokenService = tokenService;
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
                                @password_salt,
                                @phone,
                                @org_id,
                                @role_id,
                                @profile_image_url,
                                @isactive,
                                @isverified,
                                @createdby
                            )";

                await using var command = new NpgsqlCommand(query, connection);

                var PasswordhashSalt = PasswordHelper.CreatePasswordHash(userRegistration.Password);

                command.Parameters.AddWithValue("@userid", userRegistration.Id);
                command.Parameters.AddWithValue("@fname", userRegistration.Fname);
                command.Parameters.AddWithValue("@lname", userRegistration.Lname);
                command.Parameters.AddWithValue("@email", userRegistration.Email);
                command.Parameters.AddWithValue("@password_hash", PasswordhashSalt.PasswordHash);
                command.Parameters.AddWithValue("@password_salt", PasswordhashSalt.PasswordSalt);
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


        // Update User => 
        public async Task<Res<object>> UpdateUserRepo(UserRegistration userRegistration)
        {
            try
            {
                if (userRegistration == null)
                {
                    return new Res<object>(500, "No Data Found To Insert");
                }

                await using var con = new NpgsqlConnection(_connectionString);

                await con.OpenAsync();

                var query = @"SELECT public.user_update(
	                            @userid,
	                            @fname,
	                            @lname,
	                            @mail,
	                            @phone,
	                            @org_id,
	                            @role_id,
	                            @profile_image,
	                            @is_active,
	                            @is_verified,
	                            @created_by
                            )
                             ";

                await using var command = new NpgsqlCommand(query, con);

                command.Parameters.AddWithValue("@userid",userRegistration.Id);
                command.Parameters.AddWithValue("@fname",userRegistration.Fname);
                command.Parameters.AddWithValue("@lname",userRegistration.Lname);
                command.Parameters.AddWithValue("@mail",userRegistration.Email);
                //command.Parameters.AddWithValue("@password_hash",userRegistration.Password);
                command.Parameters.AddWithValue("@phone",userRegistration.PhoneNumber);
                command.Parameters.AddWithValue("@org_id",userRegistration.OrgId);
                command.Parameters.AddWithValue("@role_id",userRegistration.RoleId);
                command.Parameters.AddWithValue("@profile_image",userRegistration.ProfileImageUrl);
                command.Parameters.AddWithValue("@is_active",userRegistration.IsActive);
                command.Parameters.AddWithValue("@is_verified",userRegistration.IsVerified);
               // command.Parameters.AddWithValue("@created_by",userRegistration.CreatedBy);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var resultRecord = reader.GetFieldValue<object[]>(0); // Entire record comes as a single object[]
                    var statusCode = (int)resultRecord[0];
                    var message = (string)resultRecord[1];
                    var userId = resultRecord[2] == DBNull.Value || resultRecord[2] == null ? (int?)null : (int)resultRecord[2];


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
            catch(Exception ex) 
            {
                Log.Error(ex, "AccountRepository Error =>", userRegistration);
                return new Res<object>(500, $"Error: {ex.Message}");
            }
        }


        // UserLoginRepo 
        public async Task<Res<object>> UserLoginRepo(UserLogin userLogin)
        {
            try
            {
                if(userLogin == null || string.IsNullOrWhiteSpace(userLogin.Email))
                {
                    return new Res<object>(400, "Email is required.");
                }

                await using var con = new NpgsqlConnection(_connectionString);

                await con.OpenAsync();

                var query = @"
                               select id,password_salt,password_hash,id,email,phone_number,org_id,role_id,profile_image_url,is_active,is_verified,created_by from public.users
                               where email = @email and is_active = true LIMIT 1
                            ";

                await using var command = new NpgsqlCommand(query, con);

                command.Parameters.AddWithValue("@email", userLogin.Email);

                await using var reader = await command.ExecuteReaderAsync();

                DataTable dt = new DataTable();

                dt.Load(reader);

                if (dt.Rows.Count == 0)
                {
                    return new Res<object>(404, "User not found or inactive.");
                }

                var passwordSalt = dt.Rows[0]["password_salt"].ToString().Trim();
                var passwordHash = dt.Rows[0]["password_hash"].ToString().Trim();
                var role_id = dt.Rows[0]["role_id"].ToString().Trim();

                if (!PasswordHelper.VerifyPassword(userLogin.Password, passwordHash, passwordSalt))
                {
                    return new Res<object>(401, "Invalid password.");
                }

                var obj = (from DataRow row in dt.Rows
                           select new
                           {
                               id = row["id"].ToString(),
                               email = row["email"].ToString(),
                               phone = row["phone_number"].ToString(),
                               org_id = row["org_id"].ToString(),
                               role_id = row["role_id"].ToString(),
                               profile_image_url = row["profile_image_url"].ToString(),
                               isactive = Convert.ToBoolean(row["is_active"]),
                               isverified = Convert.ToBoolean(row["is_verified"]),
                               createdby = row["created_by"] != DBNull.Value ? row["created_by"].ToString() : null
                           });

                string token = _tokenService.GenerateToken(userLogin.Email, role_id,"");

                var jsonData = new
                {
                    UserData = obj,
                    token = token
                };

                return new Res<object>(200, "Login success", jsonData);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "AccountRepository Login Error =>", userLogin);
                return new Res<object>(500, $"Error: {ex.Message}");
            }
        }













    }
}
