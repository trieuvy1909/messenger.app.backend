namespace MessengerApplication.Dtos;

public class CreateMessageDto
{
    public string ChatId { get; set; }
    public string Sender { get; set; } 
    public string Payload { get; set; }
    public string Date { get; set; }
}