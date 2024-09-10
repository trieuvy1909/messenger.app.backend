using MessengerApplication.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace MessengerApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMemoryCache _memoryCache;

        public ChatHub(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public override async Task OnConnectedAsync()
        {
            var contextHttp = Context.GetHttpContext();
            var userId = contextHttp.Request.Query["token"].ToString();
            var connectionId = Context.ConnectionId;

            string space = null;

            if (!_memoryCache.TryGetValue(userId, out space))
            {
                _memoryCache.Set(userId, connectionId);
            }
            else
            {
                _memoryCache.Remove(userId);
                _memoryCache.Set(userId, connectionId);
            }

            Console.WriteLine("After adding:" + _memoryCache.Get(userId));
            
            await base.OnConnectedAsync();
        }     

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.GetHttpContext().Request.Query["token"].ToString();
            Console.WriteLine("Before delete:" + _memoryCache.Get(userId));
            _memoryCache.Remove(userId);

            Console.WriteLine("After delete: " + _memoryCache.Get(userId));

            await base.OnDisconnectedAsync(exception);
        }
    }
}
