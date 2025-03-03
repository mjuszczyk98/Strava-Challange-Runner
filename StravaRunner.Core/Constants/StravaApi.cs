namespace StravaRunner.Core.Constants;

public static class StravaApi
{
    public const string BaseUrl = "https://www.strava.com/api/v3";
    public const string ClubUrlTemplate = $"{BaseUrl}/clubs/{{0}}/activities";
    public const string ClubActivitiesUrlTemplate = $"{BaseUrl}/clubs/{{0}}/activities";
    public const string ClubMembersUrlTemplate = $"{BaseUrl}/clubs/{{0}}/members";
    public const string OauthRefreshUrl = $"{BaseUrl}/oauth/token";
    public const string ODataAuthorizeUrl =
        $"{BaseUrl}/oauth/authorize?client_id={{0}}&redirect_uri={{1}}&scope={{2}}&state={{3}}&approval_prompt=force&response_type=code";
    
    public static string GetClubUrl(string clubId) => string.Format(ClubUrlTemplate, clubId);
    public static string GetClubActivitiesUrl(string clubId) => string.Format(ClubActivitiesUrlTemplate, clubId);
    public static string GetClubMembersUrl(string clubId) => string.Format(ClubMembersUrlTemplate, clubId);
    public static string GetAuthorizeUrl(string clubId, string redirectUrl, string scope, string? state = "") =>
        string.Format(ODataAuthorizeUrl, clubId, redirectUrl, scope, state);
    
    public static class Scopes
    {
        public const string Read = "read";
        public const string ReadAll = "read_all";
        public const string ProfileRead = "profile:read_all";
        public const string ProfileWrite = "profile:write";
        public const string ActivityRead = "activity:read";
        public const string ActivityReadAll = "activity:read_all";
        public const string ActivityWrite = "activity:write";
    }

}