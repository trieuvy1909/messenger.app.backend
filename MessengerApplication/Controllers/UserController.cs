using System.IdentityModel.Tokens.Jwt;
using MessengerApplication.Dtos;
using MessengerApplication.Models;
using MessengerApplication.Parameters;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MessengerApplication.Helper;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/users/")]
public class UserController : ControllerBase
{
   private readonly UsersService _usersService;
   private readonly IHttpContextAccessor _httpContextAccessor;
   
   public UserController(UsersService usersService,IHttpContextAccessor httpContextAccessor)
   {
      _usersService = usersService;
      _httpContextAccessor = httpContextAccessor;
   }
   
   [HttpGet("user")]
   [Authorize]
   public async Task<IActionResult> GetUser(string id)
   {
      var user = await _usersService.GetUserAsync(id);

      return Ok(user);
   }

   [HttpGet("users")]
   [Authorize]
   public async Task<IActionResult> GetAllUsersExcept(string userId)
   {
      var users = await _usersService.GetAllExceptAsync(userId);

      return Ok(users);
   }

   [HttpPost("create")]
   public async Task<IActionResult> CreateUser(UserDto parameter)
   {
      if (string.IsNullOrWhiteSpace(parameter.UserName))
      {
         return BadRequest("Username is required.");
      }

      if (string.IsNullOrWhiteSpace(parameter.Password))
      {
         return BadRequest("Password is required.");
      }
      if (string.IsNullOrWhiteSpace(parameter.FullName))
      {
         return BadRequest("FullName is required.");
      }
      var existingUser = await _usersService.GetUserByUsernameAsync(parameter.UserName);
      if (existingUser != null)
      {
         return Conflict("A user with this username already exists.");
      }

      var newUser = new User
      {
         UserName = parameter.UserName.Trim(),
         FullName = parameter.FullName.Trim(),
         Password = PasswordHasher.HashPassword(parameter.Password)
      };

      await _usersService.CreateAsync(newUser);

      return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, "User registered successfully.");
   }
   [HttpPost("login")]
   public async Task<IActionResult> Login(UserDto parameter)
   {
      if (string.IsNullOrWhiteSpace(parameter.UserName))
      {
         return BadRequest("Username is required.");
      }
      if (string.IsNullOrWhiteSpace(parameter.Password))
      {
         return BadRequest("Password is required.");
      }
      var user = await _usersService.GetUserByUsernameAsync(parameter.UserName);

      if (user == null || !PasswordHasher.VerifyPassword(parameter.Password, user.Password))
      {
         return Unauthorized("Invalid username or password.");
      }

      var token = JwtToken.GenerateJwtToken(user,_httpContextAccessor);

      return Ok(new { message = "Login successful.", token });
   }
   
   [HttpGet("my-info")]
   [Authorize]
   public async Task<IActionResult> GetMyInfo()
   {
      var userName = User.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;

      if (userName == null)
      {
         return Unauthorized(new { Message = "Không thể lấy thông tin người dùng." });
      }
      var user = await _usersService.GetUserByUsernameAsync(userName);
      if (user != null)
      {
         user.Password = "";
         return Ok(user);
      }
      else
      {
         return BadRequest("User not found.");
      }
   }
}