using ENT.Helpers;

namespace ENT.Repository.RBAC
{
    public interface IRBACRepository
    {
        Task<Res<object>> GetModules(string moduleId);
    }
}
