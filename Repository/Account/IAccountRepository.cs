using ENT.Helpers;
using ENT.Models;

namespace ENT.Repository.Account
{
    public interface IAccountRepository
    {
        Task<Res<object>> RegisterUser(UserRegistration userRegistration);
        Task<Res<object>> UpdateUserRepo(UserRegistration userRegistration);
    }
}
