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
    public string? Type { get; set; }
    public string? Avatar { get; set; } = null!;
    public Message? PinnedMessage { get; set; }
    public bool PinnedMessageHidden { get; set; } = false;
    public Message? ReplyMessage { get; set; }
    public int Unread { get;set; } = 0;
    public string DraftMessage { get; set; } = "";
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
}