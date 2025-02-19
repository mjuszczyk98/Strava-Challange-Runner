namespace StravaRunner.Constants;

public static class StravaApi
{
    public const string BaseUrl = "https://www.strava.com/api/v3";
    public const string ClubUrlTemplate = $"{BaseUrl}/clubs/{{0}}/activities";
    public const string ClubActivitiesUrlTemplate = $"{BaseUrl}/clubs/{{0}}/activities";
    public const string ClubMembersUrlTemplate = $"{BaseUrl}/clubs/{{0}}/members";
    public const string RefreshTokenUrl = $"{BaseUrl}/oauth/token";
    
    public static string GetClubUrl(string clubId) => string.Format(ClubUrlTemplate, clubId);
    public static string GetClubActivitiesUrl(string clubId) => string.Format(ClubActivitiesUrlTemplate, clubId);
    public static string GetClubMembersUrl(string clubId) => string.Format(ClubMembersUrlTemplate, clubId);
}