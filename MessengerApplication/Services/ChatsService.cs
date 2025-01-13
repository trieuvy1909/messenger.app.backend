using MessengerApplication.Dtos;
using MessengerApplication.Models;
using MessengerApplication.Services.Interface;
using MongoDB.Driver;

namespace MessengerApplication.Services;

public class ChatsService : IChatsService
{
    private readonly IMongoCollection<Chat> _chats;
    private readonly IUsersService _usersService;
    private readonly  Lazy<IMessagesService> _messagesService;

    public ChatsService(DatabaseProviderService databaseProvider, 
        IUsersService usersService,  Lazy<IMessagesService> messagesService)
    {
        var mongoDatabase = databaseProvider.GetAccess();
        _chats = mongoDatabase.GetCollection<Chat>("Chats");
        _messagesService = messagesService;
        _usersService = usersService;
    }
    public async Task<List<Chat>> GetChatsOfUsersAsync(string userId)
    {
        var filter = Builders<Chat>.Filter.ElemMatch(chat => chat.Members, user => user.Id == userId);
        var chats = await _chats.Find(filter).ToListAsync();

        foreach (var chat in chats.ToList())
        {
            var messages = await _messagesService.Value.GetMessagesAsync(chat.Id);
            if(messages.Count > 0)
            {
                chat.Messages = messages;
                chat.LastMessage = messages.LastOrDefault();
            }
            else
            {
                chats.Remove(chat);
            }
        }
        return chats;
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

        var title = "";
        title += initiator.Profile.FullName;
        title = users.Aggregate(title, (current, user) => current + (", " + user.Profile.FullName));
        
        // Tạo đối tượng chat với các người dùng
        var chat = new Chat
        {
            Name = chatDto.Name ?? title,
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
        await _messagesService.Value.DeleteMessagesAsync(chatId);
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
    public async Task DeleteUserFromChatAsync(ChatDto parameter)
    {
        var chat = await _chats.Find(x => x.Id.Equals(parameter.ChatId)).FirstOrDefaultAsync();
        if (chat == null)
        {
            throw new ArgumentException("Chat not found.");
        }

        foreach (var recipient in parameter.Recipients)
        {
            if (!chat.Members.Any(user => user.Id.Equals(recipient)))
            {
                throw new ArgumentException("User is not in chat.");
            }
            chat.Members.RemoveAll(member => member.Id == recipient);
        }
        await _chats.ReplaceOneAsync(x => x.Id.Equals(parameter.ChatId), chat);
        foreach (var recipient in parameter.Recipients)
        {
            await _usersService.DeleteUsersChatsAsync(new AddChatDto
            {
                UserId = recipient,
                ChatId = chat.Id
            });
        }
    }
    public async Task<List<UserSummary>> GetRecipient(UserSummary sender, string chatId)
    {
        var chats = _chats.Find(x => x.Id != null && x.Id.Equals(chatId)).FirstOrDefault();
        if(chats == null) throw new ArgumentException("Chat not found");
        return chats.Members
            .Where(member => member.Id != sender.Id)
            .ToList();
    }
}