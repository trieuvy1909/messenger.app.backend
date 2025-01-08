using MessengerApplication.Dtos;
using MessengerApplication.Services;
using MessengerApplication.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/chats/")]
public class ChatController : ControllerBase
{
  private readonly IChatsService _chatsService;
  
  public ChatController(IChatsService chatsService)
  {
    _chatsService = chatsService;
  }
  
  [Authorize]
  [HttpGet("get-chats")]
  public async Task<IActionResult> GetAllChats()
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("You must be logged in to add a user to a chat.");
    }
    try
    {
      var chats = await _chatsService.GetChatsOfUsersAsync(userId);
      
      return Ok(chats);
    }
    catch (ArgumentException e)
    {
      return NotFound(e.Message);
    }
  }
  
  [Authorize]
  [HttpPost("create")]
  public async Task<IActionResult> CreateChat(ChatDto parameter)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("You must be logged in to add a user to a chat.");
    }
    if (parameter.Recipients == null || parameter.Recipients.Count == 0)
    {
      return BadRequest("Recipients are required.");
    }
    try
    {
      var chatDto = new ChatDto
      {
        Initiator = userId,
        Recipients = parameter.Recipients,
        Name = parameter.Name
      };
      return Ok(await _chatsService.CreateChatAsync(chatDto));
    }
    catch (Exception e)
    {
      return BadRequest(e.Message);
    }
  }
  
  [Authorize]
  [HttpDelete]
  public async Task<IActionResult> DeleteChat(string id)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("You must be logged in to delete a chat.");
    }
    try
    {
      await _chatsService.DeleteChatAsync(id, userId);
      return Ok(new { message = "Chat deleted" });
    }
    catch (UnauthorizedAccessException ex)
    {
      return StatusCode(403, new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return NotFound(ex.Message);
    }
    catch (Exception ex)
    {
      return StatusCode(500, "An unexpected error occurred: " + ex.Message);
    }
  }
  
  [Authorize]
  [HttpPost("add-user-to-chat")]
  public async Task<IActionResult> AddUserToChat(ChatDto parameter)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("You must be logged in to add a user to a chat.");
    }
    if (string.IsNullOrEmpty(parameter.ChatId))
    {
      return BadRequest("Chat ID is required.");
    }
    if (parameter.Recipients == null || parameter.Recipients.Count == 0)
    {
      return BadRequest("Recipients are required.");
    }
    try
    {
      await _chatsService.AddUserToChatAsync(parameter);
      return Ok(new { message = "User added to chat" });
    }
    catch (UnauthorizedAccessException ex)
    {
      return StatusCode(403, new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return NotFound(ex.Message);
    }
    catch (Exception ex)
    {
      return StatusCode(500, "An unexpected error occurred: " + ex.Message);
    }
  }

  [Authorize]
  [HttpDelete("delete-user-from-chat")]
  public async Task<IActionResult> DeleteUserFromChat(ChatDto parameter)
  {
    var userId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("You must be logged in to add a user to a chat.");
    }
    if (string.IsNullOrEmpty(parameter.ChatId))
    {
      return BadRequest("Chat ID is required.");
    }
    if (parameter.Recipients == null || parameter.Recipients.Count == 0)
    {
      return BadRequest("Recipients are required.");
    }
    try
    {
      await _chatsService.DeleteUserFromChatAsync(parameter);
      return Ok(new { message = "User deleted to chat" });
    }
    catch (UnauthorizedAccessException ex)
    {
      return StatusCode(403, new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return NotFound(ex.Message);
    }
    catch (Exception ex)
    {
      return StatusCode(500, "An unexpected error occurred: " + ex.Message);
    }
  }
}