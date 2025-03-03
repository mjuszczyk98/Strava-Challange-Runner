namespace StravaRunner.Core.Models.Data;

public class LoginToken : Entity
{
    public required string UserId { get; set; }
    public required byte[] TokenHash { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsValid => !IsUsed && ExpiresAt > DateTimeOffset.UtcNow;
}