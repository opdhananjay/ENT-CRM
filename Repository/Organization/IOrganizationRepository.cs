using ENT.Helpers;
using ENT.Models;

namespace ENT.Repository.Organization
{
    public interface IOrganizationRepository
    {
        Task<Res<object>> CreateOrUpdateOrganization(OrganizationCU organization);

        Task<Res<object>> GenerateRolesRepo(int orgId);

        Task<int> GetAdminRoleId(int orgId);
    }
}
