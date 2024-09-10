using MessengerApplication.Dtos;
using MessengerApplication.Parameters;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/messenger/")]
public class MessengerController : ControllerBase
{
  private readonly ChatsService _chatsService;
  
  public MessengerController(ChatsService chatsService)
  {
    _chatsService = chatsService;
  }

  [HttpGet("chats")]
  public async Task<IActionResult> GetAllChats(string userId)
  {
    try
    {
      var chats = await _chatsService.GetUsersChatsAsync(userId);
      
      return Ok(chats);
    }
    catch (ArgumentException e)
    {
      return NotFound(e.Message);
    }
  }

  [HttpPost("create")]
  public async Task<IActionResult> CreateChat(CreateChatParameter parameter)
  {
    try
    {
      var chatDto = new CreateChatDto
      {
        Initiator = parameter.Initiator,
        Recipient = parameter.Recipient
      };

      await _chatsService.CreateChatAsync(chatDto);

      return Ok();
    }
    catch (ArgumentException e)
    {
      return Ok(e.Message);
    }
  }
}