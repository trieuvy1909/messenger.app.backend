using MessengerApplication.Dtos;
using MessengerApplication.Services;
using MessengerApplication.Services.Interface;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/message/")]
public class MessageController : ControllerBase
{
   private readonly IMessagesService _messagesService;
   private readonly IUsersService _usersService;
   public MessageController(IMessagesService messagesService, IUsersService usersService)
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