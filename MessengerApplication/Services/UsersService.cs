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

    public async Task<(List<User> Users, long TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var users = await _users
            .Find(new BsonDocument())
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Project(u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                Profile = u.Profile,
                Chats = u.Chats,
                Status = u.Status
            })
            .ToListAsync();
        var totalCount = await _users.CountDocumentsAsync(new BsonDocument());
        return (users, totalCount);
    }
    
    public async Task<User> GetUserAsync(string userId)
    {
        return _users.Find(x => x.Id.Equals(userId))
                    .Project(u => new User
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Profile = u.Profile,
                        Status = u.Status
                    })
                    .FirstOrDefault();
    }
    public async Task<UserSummary> GetUserSummaryAsync(string userId)
    {
        return _users.Find(x => x.Id.Equals(userId))
            .Project(u => new UserSummary
            {
                Id = u.Id,
                UserName = u.UserName,
                Profile = u.Profile,
                Status = u.Status,
            })
            .FirstOrDefault();
    }
    public User GetUser(string userId) => _users.Find(x => x.Id.Equals(userId)).FirstOrDefault();
    
    public async Task EditUsersChatsAsync(AddChatDto chatDto)
    {
        var user = await _users.Find(x=>x.Id.Equals(chatDto.UserId)).SingleAsync();
        user.Chats.Add(chatDto.ChatId);

        await _users.ReplaceOneAsync(x=>x.Id.Equals(chatDto.UserId), user);
    }
    public async Task DeleteUsersChatsAsync(AddChatDto chatDto)
    {
        var user = await _users.Find(x=>x.Id.Equals(chatDto.UserId)).SingleAsync();
        if (user.Chats.Contains(chatDto.ChatId))
        {
            user.Chats.Remove(chatDto.ChatId); // Loại bỏ ChatId khỏi danh sách Chats
        }
        else
        {
            throw new ArgumentException("Chat ID not found in user's chats");
        }
        await _users.ReplaceOneAsync(x => x.Id.Equals(chatDto.UserId), user);
    }
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();
    }
}