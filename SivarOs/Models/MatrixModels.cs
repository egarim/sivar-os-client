namespace SivarOs.Models;

public class MatrixRoom
{
    public string RoomId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Topic { get; set; } = "";
    public string LastMessage { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public int UnreadCount { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class MatrixMessage
{
    public string EventId { get; set; } = "";
    public string SenderId { get; set; } = "";
    public string SenderName => SenderId.Split(':')[0].TrimStart('@');
    public string Body { get; set; } = "";
    public string MsgType { get; set; } = "m.text";
    public long Timestamp { get; set; }
    public DateTime SentAt => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).LocalDateTime;
    public bool IsOwn { get; set; }
}

public class MatrixUser
{
    public string UserId { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public string HomeServer { get; set; } = "";
    public string DisplayName => UserId.Split(':')[0].TrimStart('@');
}
