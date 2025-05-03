using ENT.Helpers;
using ENT.Models;
using ENT.Repository.Organization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ENT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationController(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        [HttpPost("OrgCreateOrUpdate")]
        public async Task<IActionResult> OrganizationCreateOrUpdate(OrganizationCU organizationCU)
        {
            var response = await _organizationRepository.CreateOrUpdateOrganization(organizationCU);
            
            if(response.StatusCode == 200)
            {
                return Ok(response);
            }

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("GenerateRoles")]
        public async Task<IActionResult> GenerateRolesOrg(int orgId)
        {
            if(orgId <= 0)
            {
                return StatusCode(400, new Res<object>(400, "Please provide organization id"));
            }

            var response = await _organizationRepository.GenerateRolesRepo(orgId);

            if(response.StatusCode == 200)
            {
                return Ok(response);
            }

            return StatusCode(response.StatusCode,response);
        }

        [HttpPost("GetAdminRoleIdOrg")]
        public async Task<IActionResult> GetAdminRoleIdOrg(int OrgId)
        {
            var response = await _organizationRepository.GetAdminRoleId(OrgId);
            return Ok(response);
        }


    }
}
