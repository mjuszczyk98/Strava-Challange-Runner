namespace StravaRunner.API.Models.Auth;

public record StravaAuthRequest(string State, string Code);