using System.IdentityModel.Tokens.Jwt;
using MessengerApplication.Helper;
using MessengerApplication.Models;
using MessengerApplication.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace MessengerApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ChatsService _chatsService;

        public ChatHub(IMemoryCache memoryCache,IHttpContextAccessor httpContextAccessor,IHubContext<ChatHub> hubContext,ChatsService chatsService)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
            _chatsService = chatsService;
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
                    Console.WriteLine($"UserId: {userId}");
                    if (!_memoryCache.TryGetValue(userId, out List<string>? connectionIds))
                    {
                        connectionIds = new List<string>();
                    }
                    connectionIds?.Add(Context.ConnectionId);
                    _memoryCache.Set(userId, connectionIds);

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
                    Console.WriteLine($"UserId: {userId} is disconnecting.");
                    if (_memoryCache.TryGetValue(userId, out List<string>? connectionIds))
                    {
                        connectionIds?.Remove(Context.ConnectionId);
                        if (connectionIds != null && connectionIds.Count == 0)
                        {
                            _memoryCache.Remove(userId);
                        }
                        else
                        {
                            _memoryCache.Set(userId, connectionIds);
                        }
                    }
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
        public async Task SendMessageAsync(Message message)
        {
            if (_memoryCache.TryGetValue(message.Recipient.Id, out List<string>? connectionIds))
            {
                if (connectionIds != null)
                {
                    foreach (var connectionId in connectionIds)
                    {
                        try
                        {
                            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException($"Error sending message: {ex.Message}");
                        }
                    }
                }
            }
        }
        // Phương thức gửi tin nhắn đến một vài người dùng
        public async Task SendMessageToUsers(List<string> userIds, string message)
        {
            foreach (var userId in userIds)
            {
                var connectionId = _memoryCache.Get<string>(userId);
                if (connectionId != null)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }
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