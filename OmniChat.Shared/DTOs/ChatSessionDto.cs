namespace OmniChat.Shared.DTOs;

public class ChatSessionDto
{
    public Guid UserId { get; set; }
    public string UserPhone { get; set; }
    public string LastMessagePreview { get; set; }
}

public class MessageDto
{
    public string Content { get; set; }
    public bool IsBot { get; set; }
}