namespace StravaRunner.API.Models;

// Login
public record LoginRequest(string Email);

// Strava
public record StravaAuthRequest(string State, string Code);

// Authenticate
public record AuthenticateRequest(string Token);

// Refresh
public record RefreshTokenRequest(string RefreshToken);