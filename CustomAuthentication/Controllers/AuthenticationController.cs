using CustomAuthentication.Data;
using CustomAuthentication.Data.Models;
using CustomAuthentication.Data.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CustomAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterVm registerVm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all the required fields");
            }
            var userExists = await _userManager.FindByEmailAsync(registerVm.Email);
            if (userExists != null)
            {
                return BadRequest($"The User {registerVm.Email} already esists");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = registerVm.UserName,
                Email = registerVm.Email,
                FirstName = registerVm.FirstName,
                LastName = registerVm.LastName,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(newUser, registerVm.Password);

            if (result.Succeeded) return Ok("User Created");

            return BadRequest("User could not be created");
        }
    }
}
