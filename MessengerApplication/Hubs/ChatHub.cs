using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using MessengerApplication.Helper;
using MessengerApplication.Models;
using MessengerApplication.Services;
using MessengerApplication.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace MessengerApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ConnectionMapping _connectionMapping;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatsService _chatsService;

        public ChatHub(IHttpContextAccessor httpContextAccessor,IHubContext<ChatHub> hubContext, 
            IChatsService chatsService, ConnectionMapping connectionMapping)
        {
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
            _chatsService = chatsService;
            _connectionMapping = connectionMapping;
        }

        // Lưu ConnectionId vào bộ nhớ khi người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"]
                        ?? _httpContextAccessor.HttpContext?.Request.Query.Where(q=>q.Key == "access_token").Select(q=>q.Value).FirstOrDefault();

            if (!string.IsNullOrEmpty(token))
            {
                var userId = JwtToken.GetUserIdFromToken(token); 
                if (!string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"UserId: {userId} is connecting to {Context.ConnectionId}");
                    _connectionMapping.Add(userId, Context.ConnectionId);

                    var chatIds = await _chatsService.GetAllGroupChatId(userId);
                    foreach (var chatId in chatIds)
                    {
                        await JoinGroup(chatId);
                    }
                }
            }

            await base.OnConnectedAsync();
        }


        // Xóa ConnectionId khỏi bộ nhớ khi người dùng rời khỏi
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"]
                        ?? _httpContextAccessor.HttpContext?.Request.Query.Where(q=>q.Key == "access_token").Select(q=>q.Value).FirstOrDefault();
            
            if (!string.IsNullOrEmpty(token))
            {
                var userId = JwtToken.GetUserIdFromToken(token); 
                if (!string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"UserId: {userId} is disconnecting from {Context.ConnectionId}");
                    _connectionMapping.Remove(userId, Context.ConnectionId);
                    
                    var chatIds = await _chatsService.GetAllGroupChatId(userId);
                    foreach (var chatId in chatIds)
                    {
                        await LeaveGroup(chatId);
                    }
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Gửi tin nhắn tới tất cả người dùng
        public async Task SendMessageToAll(Message message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error sending message: {ex.Message}");
            }
        }
        
        // Phương thức gửi tin nhắn đến một người dùng
        public async Task SendMessageAsync(Message message,string senderId, string recipientId)
        {
            var senderConnectionIds = _connectionMapping.GetConnections(senderId);
            var connectionIds = _connectionMapping.GetConnections(recipientId);
            if (senderConnectionIds != null) connectionIds?.AddRange(senderConnectionIds);

            if (connectionIds == null || connectionIds.Count == 0)
            {
                return;
            }

            // Khởi tạo và thực thi tác vụ gửi tin nhắn
            var tasks = connectionIds.Select(async connectionId =>
            {
                try
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
                catch (Exception ex)
                {
                    // Log lỗi cho từng kết nối
                    Console.WriteLine($"Error sending message to connection {connectionId}: {ex.Message}");
                }
            });

            // Chờ tất cả tác vụ hoàn thành
            await Task.WhenAll(tasks);
        }
        
        private async Task JoinGroup(string chatId)
        {
            try
            {
                await _hubContext.Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                Console.WriteLine($"{Context.ConnectionId} joined group {chatId}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error join group: {ex.Message}");
            }
        }

        private async Task LeaveGroup(string chatId)
        {
            try
            {
                await _hubContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
                Console.WriteLine($"{Context.ConnectionId} left group {chatId}");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error leave group: {ex.Message}");
            }
            
        }

        // Gửi tin nhắn đến nhóm chat
        public async Task SendMessageToGroup(Message message)
        {
            try
            {
                await _hubContext.Clients.Group(message.ChatId).SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error sending message to group: {ex.Message}");
            }
        }
    }
}