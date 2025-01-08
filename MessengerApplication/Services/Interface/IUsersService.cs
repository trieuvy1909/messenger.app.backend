using MessengerApplication.Dtos;
using MessengerApplication.Models;

namespace MessengerApplication.Services.Interface;

public interface IUsersService
{
    Task CreateAsync(User newUser);
    Task<(List<User> Users, long TotalCount)> GetAllAsync(int page, int pageSize);
    Task<User> GetUserAsync(string userId);
    Task<UserSummary> GetUserSummaryAsync(string userId);
    User GetUser(string userId);
    Task EditUsersChatsAsync(AddChatDto chatDto);
    Task DeleteUsersChatsAsync(AddChatDto chatDto);
    Task<User?> GetUserByUsernameAsync(string username);
}