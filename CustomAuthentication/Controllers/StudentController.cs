using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthentication.Controllers
{
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
