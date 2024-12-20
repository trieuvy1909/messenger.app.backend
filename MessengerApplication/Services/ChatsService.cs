using MessengerApplication.Dtos;
using MessengerApplication.Models;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class ChatsService
{
    private readonly IMongoCollection<Chat> _chats;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly UsersService _usersService;

    public ChatsService(DatabaseProviderService databaseProvider, UsersService usersService)
    {
        _usersService = usersService;
        _mongoDatabase = databaseProvider.GetAccess();
        _chats = _mongoDatabase.GetCollection<Chat>("Chats");
    }
    public async Task<List<Chat>> GetChatsOfUsersAsync(string userId)
    {
        var filter = Builders<Chat>.Filter.ElemMatch(chat => chat.Members, user => user.Id == userId);
        var chats = await _chats.Find(filter)
            .Project(u => new Chat
            {
                Id = u.Id,
                Title = u.Title,
                CreatedBy = u.CreatedBy,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
        if (chats.Count is 0) throw new ArgumentException("No chats yet");
        return chats;
    }
    public async Task<Chat> GetChatOfUserById(string userId,string chatId)
    {
        var filter = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.ElemMatch(chat => chat.Members, user => user.Id == userId),
            Builders<Chat>.Filter.Eq(chat => chat.Id, chatId)
        );        
        var chat = await _chats.Find(filter)
            .FirstOrDefaultAsync();
        if (chat==null) throw new ArgumentException("No chats yet");
        return chat;
    }
    
    public async Task<Chat> CreateChatAsync(ChatDto chatDto)
    {
        // Kiểm tra xem chat giữa các người dùng đã tồn tại chưa
        var filterOne = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.Eq("Members.UserId", chatDto.Initiator),
            Builders<Chat>.Filter.Eq("Members.UserId", chatDto.Recipients)
        );
    
        // Kiểm tra nếu chat đã tồn tại thì throw exception
        var existingChat = await _chats.Find(filterOne).FirstOrDefaultAsync();
        if (existingChat != null) 
            throw new ArgumentException("Chat already created");

        // Lấy người dùng từ dịch vụ
        var users = new List<UserSummary>();

        // Thêm người khởi tạo và người nhận vào danh sách người dùng
        var initiator = new UserSummary();
        if (!string.IsNullOrEmpty( chatDto.Initiator))
        {
            initiator = await _usersService.GetUserSummaryAsync(chatDto.Initiator);
            users.Add(initiator);
        }

        if (chatDto.Recipients != null)
        {
            foreach (var recipientId in chatDto.Recipients)
            {
                var recipient = await _usersService.GetUserSummaryAsync(recipientId);
                users.Add(recipient);
            }
        }
        
        // Tạo đối tượng chat với các người dùng
        var chat = new Chat
        {
            Title = chatDto.Title,
            Members = users,
            CreatedBy = initiator
        };

        // Thêm chat vào cơ sở dữ liệu
        await _chats.InsertOneAsync(chat);

        // Cập nhật thông tin chat cho tất cả người tham gia
        foreach (var chatDtoForUser in chat.Members.Select(user => new AddChatDto()
                 {
                     UserId = user.Id,
                     ChatId = chat.Id
                 }))
        {
            await _usersService.EditUsersChatsAsync(chatDtoForUser);
        }

        return chat;
    }
    public async Task DeleteChatAsync(string chatId, string userId)
    {
        // Tìm chat trong cơ sở dữ liệu
        var chat = await _chats.Find(x => x.Id != null && x.Id.Equals(chatId)).FirstOrDefaultAsync();
        if (chat == null)
        {
            throw new ArgumentException("Chat not found.");
        }

        var isMember = chat.Members.Any(user => user.Id.Equals(userId));
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this chat.");
        }
        
        // Xóa chat khỏi cơ sở dữ liệu
        await _chats.DeleteOneAsync(x => x.Id.Equals(chatId));

        // Cập nhật lại dữ liệu của các người dùng để loại bỏ chat đã xóa
        foreach (var chatDtoForUser in chat.Members.Select(user => new AddChatDto
             {
                 UserId = user.Id,
                 ChatId = chatId
             }))
        {
            await _usersService.DeleteUsersChatsAsync(chatDtoForUser);
        }
    }
    public async Task<List<string>> GetAllGroupChatId(string userId)
    {
        var filter = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.ElemMatch(chat => chat.Members, user => user.Id == userId),
            Builders<Chat>.Filter.SizeGt(chat => chat.Members, 2)
        );
        var chats = await _chats.Find(filter)
            .Project(chat => chat.Id)
            .ToListAsync();
        return chats;
    }
    public async Task AddUserToChatAsync(ChatDto parameter)
    {
        var chat = await _chats.Find(x => x.Id.Equals(parameter.ChatId)).FirstOrDefaultAsync();
        if (chat == null)
        {
            throw new ArgumentException("Chat not found.");
        }

        foreach (var recipient in parameter.Recipients)
        {
            if (chat.Members.Any(user => user.Id.Equals(recipient)))
            {
                throw new ArgumentException("User already in chat.");
            }
            var user = await _usersService.GetUserSummaryAsync(recipient);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            chat.Members.Add(user);
            await _usersService.EditUsersChatsAsync(new AddChatDto
            {
                UserId = user.Id,
                ChatId = chat.Id
            });
        }
        await _chats.ReplaceOneAsync(x => x.Id.Equals(parameter.ChatId), chat);
    }
}