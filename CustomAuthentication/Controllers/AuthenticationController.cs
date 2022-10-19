using CustomAuthentication.Data;
using CustomAuthentication.Data.Models;
using CustomAuthentication.Data.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        private readonly TokenValidationParameters _validationParameters;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration,
            TokenValidationParameters validationParameters)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _validationParameters = validationParameters;
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

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVm loginVm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }
            var userExists = await _userManager.FindByEmailAsync(loginVm.Email);
            if(userExists != null && await _userManager.CheckPasswordAsync(userExists, loginVm.Password))
            {
                var tokenValue = await GenerateJWTTokenAsync(userExists, null);

                return Ok(tokenValue);
            }
            return Unauthorized();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVm tokenRequestVm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }
            var result = await VerifyAndGenerateTokenAsync(tokenRequestVm);

            return Ok(result);
        }

        private async Task<AuthResultsVm> VerifyAndGenerateTokenAsync(TokenRequestVm tokenRequestVm)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == tokenRequestVm.RefreshToken);

            var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
            try
            {
                var tokenCheckResult = jwtTokenHandler
                    .ValidateToken(tokenRequestVm.Token, _validationParameters, out var validatedToken);

                return await GenerateJWTTokenAsync(dbUser, storedToken);
            }
            catch (SecurityTokenExpiredException)
            {

                if(storedToken.DateExpire >= DateTime.UtcNow)
                {
                    return await GenerateJWTTokenAsync(dbUser, storedToken);
                }
                else
                {
                    return await GenerateJWTTokenAsync(dbUser, null);
                }
            }
        }

        private async Task<AuthResultsVm> GenerateJWTTokenAsync(ApplicationUser user, RefreshToken rToken)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
            };
            var authSigninKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey,SecurityAlgorithms.HmacSha256));

            var JwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            if(rToken != null)
            {
                var rTokenresponse = new AuthResultsVm()
                {
                    Token = JwtToken,
                    RefreshToken = rToken.Token,
                    ExpiresAt = token.ValidTo
                };

                return rTokenresponse;
            }

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                DateAdded = DateTime.UtcNow,
                DateExpire = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            var response = new AuthResultsVm()
            {
                Token = JwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };

            return response;
        }
    }
}
