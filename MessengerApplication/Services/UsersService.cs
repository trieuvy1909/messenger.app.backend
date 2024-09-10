using MessengerApplication.Dtos;
using MessengerApplication.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoDatabase _mongoDatabase;
    
    public UsersService(DatabaseProviderService databaseProvider)
    {
        _mongoDatabase = databaseProvider.GetAccess();
        _users = _mongoDatabase.GetCollection<User>("Users");
    }

    public async Task CreateAsync(User newUser) =>
        await _users.InsertOneAsync(newUser);

    public async Task<List<User>> GetAllExceptAsync(string userId)
    {
        var users = await _users.Find(x=>x.UserId != userId).ToListAsync();

        return users;
    }
    
    public async Task<User> GetUserAsync(string userId) => _users.Find(x => x.UserId.Equals(userId)).FirstOrDefault();
    
    public User GetUser(string userId) => _users.Find(x => x.UserId.Equals(userId)).FirstOrDefault();
    
    public async Task EditUsersChatsAsync(AddChatDto chatDto)
    {
        var user = await _users.Find(x=>x.UserId.Equals(chatDto.UserId)).SingleAsync();
        user.Chats.Add(chatDto.ChatId);

        await _users.ReplaceOneAsync(x=>x.UserId.Equals(chatDto.UserId), user);
    }
}