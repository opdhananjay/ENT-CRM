using ENT.Helpers;
using Npgsql;
using System.Data;
namespace ENT.Repository.RBAC
{
    public class RBACRepository: IRBACRepository
    {
        private readonly string _connectionString;
        public RBACRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("1st");
            // Constructor logic if needed
        }

        #region "Modules"

        public async Task<Res<object>> GetModules(string moduleId)
        {
            var query = @"SELECT * FROM modules";

            await using var con = new NpgsqlConnection(_connectionString);

            await con.OpenAsync();

            await using var cmd = new NpgsqlCommand(query, con);

            if (!string.IsNullOrEmpty(moduleId))
            {
                query += " WHERE module_id = @moduleId";
                cmd.Parameters.AddWithValue("moduleId", moduleId);
            }

            await using var reader = await cmd.ExecuteReaderAsync();

            DataTable dt = new DataTable();

            dt.Load(reader);

            if (dt.Rows.Count > 0)
            {
                var jsonData = (from DataRow row in dt.Rows select row).ToList();
                return new Res<object>(200, "Modules retrieved successfully", jsonData);
            }
            else
            {
                return new Res<object>(404, "No modules data found");
            }
        }

        #endregion "Modules"


    }
}
