using ENT.Repository.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ENT.Models;
namespace ENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }


        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(UserRegistration userRegistration)
        {
            try 
            {
                var response = await _accountRepository.RegisterUser(userRegistration);
                return Ok(response); 
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Update User Function => 
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserRegistration userRegistration)
        {
            try
            {
                var response = await _accountRepository.UpdateUserRepo(userRegistration);

                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }

                return StatusCode(response.StatusCode, response);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
































        



        
    }
}
