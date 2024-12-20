using MessengerApplication.Dtos;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/message/")]
public class MessageController : ControllerBase
{
   private readonly MessagesService _messagesService;
   private readonly UsersService _usersService;
   public MessageController(MessagesService messagesService, UsersService usersService)
   {
      _messagesService = messagesService;
      _usersService = usersService;
   }
   
   [HttpPost("send")]
   public async Task<IActionResult> SendMessage(MessageDto parameter)
   {
      var senderId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
      if (string.IsNullOrWhiteSpace(senderId))
      {
         return BadRequest("You must be logged in to send messages.");
      }
      var sender = await _usersService.GetUserSummaryAsync(senderId);
      var message = new MessageDto
      {
         ChatId = parameter.ChatId,
         Payload = parameter.Payload,
         Sender = sender
      };

      await _messagesService.CreateMessageAsync(message);
      
      return Ok();
   }

   [HttpGet("receive")]
   public async Task<IActionResult> ReceiveMessages(string chatId)
   {
      var messages = await _messagesService.GetMessagesAsync(chatId);

      return Ok(messages);
   }
}