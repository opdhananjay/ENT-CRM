using ENT.Helpers;
using ENT.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Npgsql;
using Serilog;
using System.Data;
using System.Text;

namespace ENT.Repository.Organization
{
    public class OrganizationRepository: IOrganizationRepository
    {

        private readonly string _connectionString;
        public OrganizationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("1st");
        }

        public async Task<Res<object>> CreateOrUpdateOrganization(OrganizationCU organizationCU)
        {
            try
            {
                if (organizationCU == null)
                {
                    return new Res<object>(400, "Invalid organization data provided");
                }

                const string query = @"
                                    SELECT * FROM public.organization_create_or_update(
                                        @org_id,
                                        @name,
                                        @email,
                                        @phone,
                                        @logo_url,
                                        @industry,
                                        @website
                                    );";

                await using(var con = new NpgsqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    await using(var cmd = new  NpgsqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("org_id", organizationCU.OrgId);
                        cmd.Parameters.AddWithValue("name", organizationCU.Name ?? string.Empty);
                        cmd.Parameters.AddWithValue("email", organizationCU.Email ?? string.Empty);
                        cmd.Parameters.AddWithValue("phone", organizationCU.Phone ?? string.Empty);
                        cmd.Parameters.AddWithValue("logo_url", organizationCU.LogoUrl ?? string.Empty);
                        cmd.Parameters.AddWithValue("industry", organizationCU.Industry);
                        cmd.Parameters.AddWithValue("website", organizationCU.Website ?? string.Empty);                        

                        await using(var reader = await cmd.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                var statusCode = reader.GetInt32(0);
                                var message = reader.GetString(1);
                                var orgId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

                                var result = new
                                {
                                    StatusCode = statusCode,
                                    Message = message,
                                    OrgId = orgId
                                };

                                if(statusCode == 1)
                                {
                                    return new Res<object>(200, message, result);
                                }
                                else if (statusCode == 0)
                                {
                                    return new Res<object>(404, message, result);
                                }
                                else
                                {
                                    return new Res<object>(400, message, result);
                                }
                            }

                            return new Res<object>(404, "No response from DB function");
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message, "OrganizationRepository Error =>", organizationCU);
                return new Res<object>(500, $"Error: {ex.Message}");
            }
        }


        // This function Call After Organization Create So it will add default roles in organization
        public async Task<Res<object>> GenerateRolesRepo(int orgId)
        {
            try
            {
                List<string> defaultRoles = new List<string>
                {
                    "Admin",
                    "Manager",
                    "Sales Executive",
                    "Support Agent"
                };

                var queryBuldier = new StringBuilder();
                string query = @"INSERT INTO public.roles (name, org_id, is_system_defined) VALUES";
                queryBuldier.Append(query);

                for(int i = 0;i< defaultRoles.Count;i++)
                {
                    string role = defaultRoles[i];
                    queryBuldier.Append($" ('{role}', {orgId}, true)");

                    if (i < defaultRoles.Count - 1)
                        queryBuldier.Append(",");
                    else
                        queryBuldier.Append(";");
                }

                string finalQuery = queryBuldier.ToString();

                await using(var con = new NpgsqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    await using(var cmd = new NpgsqlCommand(finalQuery, con))
                    {
                        int rowAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowAffected > 0)
                        {
                            return new Res<object>(200, "Default roles inserted successfully",new { rowAffected = rowAffected });
                        }
                        else
                        {
                            return new Res<object>(400, "failed to add default roles");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Roles Creation =>", orgId);
                return new Res<object>(500, $"Error: {ex.Message}");
            }
        }

        // Get Admin Role Id of Oeganization by Passing Organization ID => Useful
        public async Task<int> GetAdminRoleId(int orgId)
        {
            if(orgId == 0)
            {
                return 0;
            }

            bool isOrgExist = await CheckOrganizationExists(orgId);

            if (isOrgExist)
            {
                string query = $"select id from roles where org_id = @org_id AND name = 'Admin'";
                await using var con = new NpgsqlConnection(_connectionString);
                await con.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, con);
                cmd.Parameters.AddWithValue("@org_id", orgId);
                var result = await cmd.ExecuteScalarAsync();
                if(result != null && int.TryParse(result.ToString(),out int roleId))
                {
                    return roleId;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }


        // Check Organization Exist => UseFul Function 
        public async Task<bool> CheckOrganizationExists(int orgId)
        {
            try
            {
                await using var con = new NpgsqlConnection(_connectionString);
                await con.OpenAsync();
                string query = "SELECT COUNT(1) FROM public.organizations WHERE id = @orgId";
                await using var cmd = new NpgsqlCommand(query, con);
                cmd.Parameters.AddWithValue("@orgId", orgId);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                // Log the error as needed
                Log.Error(ex.Message, "CheckOrganizationExists =>", orgId);
                return false; // or throw depending on how you want to handle it
            }
        }





    }
}
