using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ENT.Repository.RBAC;

namespace ENT.Controllers.RBAC
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        public readonly IRBACRepository rBACRepository;
        public ModulesController(IRBACRepository rBACRepository)
        {
            this.rBACRepository = rBACRepository;
            // Constructor logic if needed
        }

        [HttpGet("GetModules")]
        public async Task<IActionResult> GetModules(string? ModuleId = "")
        {
            try
            {
                var response = await rBACRepository.GetModules(ModuleId);
                return response.StatusCode switch
                {
                    200 => Ok(response),
                    404 => NotFound(response),
                    _ => StatusCode(response.StatusCode, response)
                };
            }
            catch(Exception ex)
            {
                throw;
            }
        }



        

    }
}
