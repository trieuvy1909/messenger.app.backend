using MessengerApplication.Models;

namespace MessengerApplication.Dtos;

public class ChatDto
{
    public string? ChatId { get; set; }
    public string? Initiator { get; set; }
    public string? Title { get; set; }
    public List<string>? Recipients { get; set; }
    public UserSummary? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class AddChatDto
{
    public string UserId { get; set; }
    public string ChatId { get; set; }
}