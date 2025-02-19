namespace StravaRunner;

public class StravaService
{
    private const string StravaBaseUrl = "https://www.strava.com/api/v3";
    
    
    
    private static async Task<string> RefreshAccessTokenAsync(string clientId, string clientSecret, string refreshToken)
    {
        const string requestUrl = $"{StravaBaseUrl}/oauth/token";
        Dictionary<string, string> formData = new()
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        using var httpClient = new HttpClient();
        using var content = new FormUrlEncodedContent(formData);
        
        var response = await httpClient.PostAsync(requestUrl, content);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        
        return null;
    }
}