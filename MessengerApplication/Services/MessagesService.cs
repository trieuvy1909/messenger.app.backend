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
    private readonly IMongoCollection<Chat> _chats;
    private readonly ChatHub _chatHub;
    public MessagesService(DatabaseProviderService databaseProvider, IHubContext<ChatHub> hubContext, ChatsService chatsService, IMemoryCache memoryCache, ChatHub chatHub)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _messages = _mongoDatabase.GetCollection<Message>("Messages");
        _chats = _mongoDatabase.GetCollection<Chat>("Chats");
        _chatHub = chatHub;
    }

    public async Task CreateMessageAsync(MessageDto message)
    {
        var newMessage = new Message
        {
            ChatId = message.ChatId,
            Payload = message.Payload,
            Sender = message.Sender,
        };

        var recipientIds = await GetRecipientId(message.Sender, message.ChatId);

        await _messages.InsertOneAsync(newMessage);

        try
        {
            if (recipientIds.Count == 1)
            {
                await _chatHub.SendMessageAsync(recipientIds.First(), newMessage.Payload);
            }
            else if(recipientIds.Count > 1)
            {
                await _chatHub.SendMessageToUsers(recipientIds, newMessage.Payload);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public async Task<List<Message>> GetMessagesAsync(string chatId)
    {
        return await _messages.Find(x => x.ChatId.Equals(chatId))
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<string>> GetRecipientId(UserSummary sender, string chatId)
    {      
        var chat = _chats.Find(x => x.Id.Equals(chatId)).FirstOrDefault();

        if(chat is not null)
        {
            return chat.Members
                .Where(member => member.Id != sender.Id)
                .Select(member=>member.Id)
                .ToList();
        }
        else
        {
            throw new ArgumentException("No chat exists");
        }
    }
}