using MessengerApplication.Dtos;
using MessengerApplication.Hubs;
using MessengerApplication.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class MessagesService
{
    private readonly IMongoCollection<Message> _messages;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ChatsService _chatsService;
    private readonly IMemoryCache _memoryCache;
    public MessagesService(DatabaseProviderService databaseProvider, IHubContext<ChatHub> hubContext, ChatsService chatsService, IMemoryCache memoryCache)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _messages = _mongoDatabase.GetCollection<Message>("Messages");
        _hubContext = hubContext;
        _chatsService = chatsService;
        _memoryCache = memoryCache;
    }

    public async Task CreateMessageAsync(CreateMessageDto message)
    {
        var newMessage = new Message
        {
            ChatId = message.ChatId,
            Payload = message.Payload,
            Sender = message.Sender,
            Date = DateTime.Now
        };

        string recipient = await _chatsService.GetRecipientId(message.Sender, message.ChatId);

        await _messages.InsertOneAsync(newMessage);

        try
        {
            await SendMessageAsync(recipient, newMessage);
        } catch(Exception ex)
        {
            Console.WriteLine("No connection");
        }
    }

    public async Task SendMessageAsync(string userId, Message message)
    {
        string connectionId = (string)_memoryCache.Get(userId);

        if (connectionId == "") throw new ArgumentException("No user connected.");

        await _hubContext.Clients.Client(connectionId).SendAsync("SendMessage", message);
    }

    public async Task<List<Message>> GetMessagesAsync(string chatId) 
        => await _messages.Find(x => x.ChatId.Equals(chatId))
            .SortBy(x => x.Date)
            .ToListAsync();
}