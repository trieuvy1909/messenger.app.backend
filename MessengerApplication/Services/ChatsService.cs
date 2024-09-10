using MessengerApplication.Dtos;
using MessengerApplication.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class ChatsService
{
    private readonly IMongoCollection<Chat> _chats;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly UsersService _usersService;
    private readonly ILogger<ChatsService> _logger;

    public ChatsService(DatabaseProviderService databaseProvider, UsersService usersService,
        ILogger<ChatsService> logger)
    {
        _logger = logger;
        _usersService = usersService;
        _mongoDatabase = databaseProvider.GetAccess();
        _chats = _mongoDatabase.GetCollection<Chat>("Chats");
    }
    public async Task<List<Chat>> GetUsersChatsAsync(string userId)
    {
        var user = await _usersService.GetUserAsync(userId);
        
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.In("Id", user.Chats);
        
        var chats = await _chats.Find(filter).ToListAsync();

        if (chats.Count is 0) throw new ArgumentException("No chats yet");
        
        return chats;
    }

    public async Task<string> GetRecipientId(string sender, string chatId)
    {      
        var chat = _chats.Find(x => x.Id.Equals(chatId)).FirstOrDefault();

        Console.WriteLine(chat);
        if(chat is not null) {
            if (chat.Users[0].Id != sender)
            {
                return chat.Users[0].Id;
            } else
            {
                return chat.Users[1].Id;
            }
        }
        else
        {
            throw new ArgumentException("No chat exists");
        }
    }

    public async Task CreateChatAsync(CreateChatDto createChatDto)
    {
        FilterDefinition<Chat> filterOne = (Builders<Chat>.Filter.Eq("Users.0.UserId", createChatDto.Initiator)
                                           | Builders<Chat>.Filter.Eq("Users.1.UserId", createChatDto.Initiator))
                                           & (Builders<Chat>.Filter.Eq("Users.0.UserId", createChatDto.Recipient)
                                           | Builders<Chat>.Filter.Eq("Users.1.UserId", createChatDto.Recipient));
        var initCheck = _chats.Find(filterOne).FirstOrDefault();
        
        // FilterDefinition<Chat> filterTwo = Builders<Chat>.Filter.Eq("Users.0.UserId", createChatDto.Recipient)
        //                                    | Builders<Chat>.Filter.Eq("Users.1.UserId", createChatDto.Recipient);
        // var recCheck = _chats.Find(filterTwo).FirstOrDefault();
        
        if(initCheck is not null) throw new ArgumentException("Chat already created");
        
        var users = new List<User>();

        var initiator = await _usersService.GetUserAsync(createChatDto.Initiator);
        var recipient = await _usersService.GetUserAsync(createChatDto.Recipient);
        
        users.Add(initiator);
        users.Add(recipient);

        var chat = new Chat
        {
            Users = users
        };

        await _chats.InsertOneAsync(chat);

        var chatId = _chats.Find(x=> x.Id.Equals(chat.Id)).FirstOrDefault();

        var chatInitiator = new AddChatDto
        {
            UserId = createChatDto.Initiator,
            ChatId = chatId.Id
        };
        var chatRecipient = new AddChatDto
        {
            UserId = createChatDto.Recipient,
            ChatId = chatId.Id
        };
        
        await _usersService.EditUsersChatsAsync(chatInitiator);
        await _usersService.EditUsersChatsAsync(chatRecipient);
    }
}