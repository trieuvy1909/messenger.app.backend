using System.IdentityModel.Tokens.Jwt;
using MessengerApplication.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace MessengerApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _senderId;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(IMemoryCache memoryCache,IHttpContextAccessor httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        // Lưu ConnectionId vào bộ nhớ khi người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"UserId: {userId}");
                _memoryCache.Set(userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }


        // Xóa ConnectionId khỏi bộ nhớ khi người dùng rời khỏi
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"UserId: {userId} is disconnecting.");
                _memoryCache.Remove(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        
        // Phương thức gửi tin nhắn đến một người dùng
        public async Task SendMessageAsync(string userId, string message)
        {
            var connectionId = _memoryCache.Get<string>(userId);
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
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
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }
        }
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"{Context.ConnectionId} joined group {groupName}");
        }

        // Rời khỏi nhóm chat
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"{Context.ConnectionId} left group {groupName}");
        }

        // Gửi tin nhắn đến nhóm chat
        public async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }
    }
}