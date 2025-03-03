namespace StravaRunner.Core.Models;

public class SecuredTokenWrapper<T>
{
    public T? Value { get; init; }
    public string? SecuredAccessToken { get; init; }
    public string? SecuredRefreshToken { get; init; }
    public int ExpiresAt { get; init; }
    
    
    public DateTimeOffset ExpiresAtDateTime => DateTimeOffset.FromUnixTimeSeconds(ExpiresAt); 
}