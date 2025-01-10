using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Name { get; set; }
    public List<UserSummary> Members { get; set; } = new ();
    public UserSummary? CreatedBy { get; set; }
    public Message? LastMessage { get; set; } = null!;
    public List<Message>? Messages { get; set; } = [];
    public string Type { get; set; } = "couple";
    public string Avatar { get; set; } = "https://chiemtaimobile.vn/images/companies/1/%E1%BA%A2nh%20Blog/avatar-facebook-dep/Anh-avatar-hoat-hinh-de-thuong-xinh-xan.jpg?1704788263223";
    public Message? PinnedMessage { get; set; }
    public bool PinnedMessageHidden { get; set; } = false;
    public Message? ReplyMessage { get; set; }
    public int Unread { get;set; } = 0;
    [BsonElement("CreatedAt")]
    public string CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
}