using MessengerApplication.Dtos;
using MessengerApplication.Models;

namespace MessengerApplication.Services.Interface;

public interface IMessagesService
{
    Task CreateMessageAsync(MessageDto message);
    Task CreateMessageToAllAsync(MessageDto message);
    Task<List<Message>> GetMessagesAsync(string chatId);
    Task DeleteMessagesAsync(string chatId);
}