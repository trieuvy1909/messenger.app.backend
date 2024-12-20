using MessengerApplication.Models;

namespace MessengerApplication.Dtos;

public class MessageDto
{
    public string? ChatId { get; set; }
    public UserSummary? Sender { get; set; } 
    public string Payload { get; set; }
}