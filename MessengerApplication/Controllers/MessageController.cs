using MessengerApplication.Dtos;
using MessengerApplication.Parameters;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/message/")]
public class MessageController : ControllerBase
{
   private readonly MessagesService _messagesService;
   
   public MessageController(MessagesService messagesService)
   {
      _messagesService = messagesService;
   }
   
   [HttpPost("create")]
   public async Task<IActionResult> CreateMessage(CreateMessageParameter parameter)
   {

      var message = new CreateMessageDto
      {
         ChatId = parameter.ChatId,
         Payload = parameter.Payload,
         Sender = parameter.Sender
      };

      await _messagesService.CreateMessageAsync(message);
      
      return Ok();
   }

   [HttpGet("get")]
   public async Task<IActionResult> GetMessages(string chatId)
   {
      var messages = await _messagesService.GetMessagesAsync(chatId);

      return Ok(messages);
   }
}