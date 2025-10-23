using IEEE_Application.DATA.DTO;
using IEEE_Application.DATA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IEEE_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<User> userManager , RoleManager<IdentityRole> roleManager , IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterNewUser(NewUserDTO NewUser)
        {
            if (ModelState.IsValid)
            {
                User user = new User() { UserName = NewUser.Name };
                IdentityResult result = await _userManager.CreateAsync(user, NewUser.Password);
                if (result.Succeeded)
                {
                    var allowedRoles = new[] { "PUZZLE_CREATOR", "GAME_CREATOR", "GAMER", "ADMIN" };
                    if (allowedRoles.Contains(NewUser.Role))
                    {
                        if (!await _roleManager.RoleExistsAsync(NewUser.Role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(NewUser.Role));
                        }

                        await _userManager.AddToRoleAsync(user, NewUser.Role);
                    }
                    else
                    {
                        return BadRequest("Invalid role specified.");
                    }

                    return Ok("User created successfully!");

                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    user.Id,
                    user.UserName,
                    Roles = roles
                });
            }
            return Ok(userList);
        }
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok("User deleted successfully!");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return NotFound("User not found!");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUser Login_user)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(Login_user.Name);
                if (user != null)
                {
                    var check = await _userManager.CheckPasswordAsync(user, Login_user.Password);
                    if (check)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var Claims = new List<Claim>();
                        Claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        foreach (var role in roles)
                        {
                            Claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                        var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            claims: Claims,
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: sc
                            );
                        var _token = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                        };
                        return Ok(_token);
                    }
                    else
                    {
                        return BadRequest("Invalid Password!");
                    }
                }
                else
                {
                    return BadRequest("User not found!");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        

    }

}
