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
      try
      {
         var senderId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
         if (string.IsNullOrWhiteSpace(senderId))
         {
            return BadRequest("You must be logged in to send messages.");
         }

         var sender = await _usersService.GetUserSummaryAsync(senderId);
         parameter.Sender = sender;
         await _messagesService.CreateMessageAsync(parameter);
      } 
      catch (Exception ex)
      {
         return BadRequest(ex.Message);
      }
      return Ok("Gửi tin nhắn thành công");
   }

   [HttpGet("receive")]
   public async Task<IActionResult> ReceiveMessages(string chatId)
   {
      var messages = await _messagesService.GetMessagesAsync(chatId);

      return Ok(messages);
   }
   [HttpPost("send-all")]
   public async Task<IActionResult> SendMessageToAll(MessageDto parameter)
   {
      try
      {
         var senderId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
         if (string.IsNullOrWhiteSpace(senderId))
         {
            return BadRequest("You must be logged in to send messages.");
         }

         var sender = await _usersService.GetUserSummaryAsync(senderId);
         if (!sender.IsAdmin)
         {
            await _messagesService.CreateMessageToAllAsync(parameter);
         }
         else
         {
            return BadRequest("Chỉ admin mới có thể gửi tin nhắn cho tất cả người dùng.");
         }
      } 
      catch (Exception ex)
      {
         return BadRequest(ex.Message);
      }
      return Ok("Gửi tin nhắn thành công");
   }
}