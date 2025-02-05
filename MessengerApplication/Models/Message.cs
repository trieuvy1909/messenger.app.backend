﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string ChatId { get; set; } = null!;
    public UserSummary Sender { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Type { get; set; } = "text";
    [BsonElement("CreatedAt")]
    public string CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
    public int ReplyTo { get; set; } = -1;
    public string? State { get; set; } = null;
}