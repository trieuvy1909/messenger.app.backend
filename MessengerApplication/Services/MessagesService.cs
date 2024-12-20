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

        var recipients = await GetRecipient(message.Sender, message.ChatId);

        await _messages.InsertOneAsync(newMessage);

        try
        {
            if (recipients.Count == 1)
            {
                newMessage.Recipient = recipients.First();
                await _chatHub.SendMessageAsync(newMessage);
            }
            // else if(recipients.Count > 1)
            // {
            //     await _chatHub.SendMessageToUsers(recipients, newMessage.Payload);
            // }
        }
        catch (Exception ex)
        {
            throw new ArgumentException(ex.Message);
        }
    }
    public async Task CreateMessageToAllAsync(MessageDto message)
    {
        var newMessage = new Message
        {
            ChatId = message.ChatId,
            Payload = message.Payload,
            Sender = message.Sender,
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
            .SortBy(x => x.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<UserSummary>> GetRecipient(UserSummary sender, string chatId)
    {      
        var chat = _chats.Find(x => x.Id.Equals(chatId)).FirstOrDefault();

        if(chat is not null)
        {
            return chat.Members
                .Where(member => member.Id != sender.Id)
                .ToList();
        }
        else
        {
            throw new ArgumentException("No chat exists");
        }
    }
}