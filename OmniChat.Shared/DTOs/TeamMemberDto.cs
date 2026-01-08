namespace OmniChat.Shared.DTOs;

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public List<string> Departments { get; set; } = new();
    public bool IsActive { get; set; }
}