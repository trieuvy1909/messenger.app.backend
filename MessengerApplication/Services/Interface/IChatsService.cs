using MessengerApplication.Dtos;
using MessengerApplication.Models;

namespace MessengerApplication.Services.Interface;

public interface IChatsService
{
    Task<List<Chat>> GetChatsOfUsersAsync(string userId);
    Task<Chat> CreateChatAsync(ChatDto chatDto);
    Task DeleteChatAsync(string chatId, string userId);
    Task<List<string>> GetAllGroupChatId(string userId);
    Task AddUserToChatAsync(ChatDto parameter);
    Task DeleteUserFromChatAsync(ChatDto parameter);
    Task<List<UserSummary>> GetRecipient(UserSummary sender, string chatId);
}