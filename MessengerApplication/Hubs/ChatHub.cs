using System.IdentityModel.Tokens.Jwt;
using MessengerApplication.Helper;
using MessengerApplication.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace MessengerApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHub(IMemoryCache memoryCache,IHttpContextAccessor httpContextAccessor,IHubContext<ChatHub> hubContext)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
        }

        // Lưu ConnectionId vào bộ nhớ khi người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"]
                        ?? _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var userId = JwtToken.GetUserIdFromToken(token); 
                if (!string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"UserId: {userId}");
                    _memoryCache.Set(userId, Context.ConnectionId);
                }
            }

            await base.OnConnectedAsync();
        }


        // Xóa ConnectionId khỏi bộ nhớ khi người dùng rời khỏi
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"]
                        ?? _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                var userId = JwtToken.GetUserIdFromToken(token); 
                if (!string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"UserId: {userId} is disconnecting.");
                    _memoryCache.Remove(userId);
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
            var connectionId = _memoryCache.Get<string>(message.Recipient.Id);
            if (connectionId != null)
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
        public async Task JoinGroup(string groupName)
        {
            await _hubContext.Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"{Context.ConnectionId} joined group {groupName}");
        }

        // Rời khỏi nhóm chat
        public async Task LeaveGroup(string groupName)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"{Context.ConnectionId} left group {groupName}");
        }

        // Gửi tin nhắn đến nhóm chat
        public async Task SendMessageToGroup(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }
    }
}