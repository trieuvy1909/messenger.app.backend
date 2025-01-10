using MessengerApplication.Dtos;
using MessengerApplication.Hubs;
using MessengerApplication.Models;
using MessengerApplication.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class MessagesService : IMessagesService
{
    private readonly IMongoCollection<Message> _messages;
    private readonly ChatHub _chatHub;
    private readonly Lazy<IChatsService> _chatsService;

    public MessagesService(DatabaseProviderService databaseProvider, ChatHub chatHub, Lazy<IChatsService> chatsService)
    {
        var mongoDatabase = databaseProvider.GetAccess();
        _messages = mongoDatabase.GetCollection<Message>("Messages");
        _chatHub = chatHub;
        _chatsService = chatsService;
    }
    
    public async Task CreateMessageAsync(MessageDto message)
    {
        var newMessage = new Message
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ChatId = message.ChatId,
            Content = message.Content,
            Sender = message.Sender,
        };
        await _messages.InsertOneAsync(newMessage);
        var recipients = await _chatsService.Value.GetRecipient(message.Sender, message.ChatId);
        switch (recipients.Count)
        {
            case 1:
                await _chatHub.SendMessageAsync(newMessage,message.Sender.Id, recipients[0].Id);
                break;
            case > 1:
                await _chatHub.SendMessageToGroup(newMessage);
                break;
        }
    }
    public async Task CreateMessageToAllAsync(MessageDto message)
    {
        var newMessage = new Message
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ChatId = "000",
            Content = message.Content,
            Sender = new UserSummary()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Profile = new Profile(){FullName = "Admin"},
                IsAdmin = true,UserName = "admin"
            }
        };
        
        try
        {
            await _chatHub.SendMessageToAll(newMessage);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(ex.Message);
        }
    }
    public async Task<List<Message>> GetMessagesAsync(string chatId)
    {
        return await _messages.Find(x => x.ChatId.Equals(chatId))
            .ToListAsync();
    }
    public async Task DeleteMessagesAsync(string chatId)
    {
        await _messages.DeleteManyAsync(x => x.ChatId.Equals(chatId));
    }
}