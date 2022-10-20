using CustomAuthentication.Data.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthentication.Controllers
{
    [Authorize(Roles = UserRoles.Student)]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        public StudentController()
        {

        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to StudentController");
        }
    }
}
