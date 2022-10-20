using CustomAuthentication.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize(Roles = UserRoles.Manager)]
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        public ManagementController()
        {

        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to ManagementController");
        }
    }
}
