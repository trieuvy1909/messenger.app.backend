using MessengerApplication.Models;
using MessengerApplication.Parameters;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/users/")]
public class UserController : ControllerBase
{
   private readonly UsersService _usersService;
   
   public UserController(UsersService usersService)
   {
      _usersService = usersService;
   }
   
   [HttpGet("user")]
   public async Task<IActionResult> GetUser(string id)
   {
      var user = await _usersService.GetUserAsync(id);

      return Ok(user);
   }

   [HttpGet("users")]
   public async Task<IActionResult> GetAllUsersExcept(string userId)
   {
      var users = await _usersService.GetAllExceptAsync(userId);

      return Ok(users);
   }

   [HttpPost("create")]
   public async Task<IActionResult> CreateUser(UserCreateParameter parameter)
   {
      var user = new User
      {
         Username = parameter.Username,
         UserId = parameter.Id
      };
      
      await _usersService.CreateAsync(user);

      return Ok();
   }

}