using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StravaRunner.Constants;
using StravaRunner.Models.Settings;
using StravaRunner.Models.Strava;

namespace StravaRunner.Services;

public interface IStravaService
{
    Task<List<Activity>> GetActivitiesAsync(string clubId);
}

public class StravaService : IStravaService
{
    private readonly IOptions<StravaTokenSettings> _stravaSettings;
    private static string _accessToken = string.Empty;

    public StravaService(IOptions<StravaTokenSettings> stravaSettings)
    {
        _stravaSettings = stravaSettings;
        _accessToken = stravaSettings.Value.AccessToken;
    }

    public Task<List<Activity>> GetActivitiesAsync(string clubId) =>
        GetFromApiWithRetryAsync<List<Activity>>(StravaApi.GetClubUrl(clubId))!;

    private async Task<T?> GetFromApiWithRetryAsync<T>(string url)
        where T : class
    {
        var response = await GetFromApiOrDefaultAsync<T>(url);
        
        if (response is not null) return response;
        
        await RefreshTokenAsync();
        return (await GetFromApiOrDefaultAsync<T>(url))!;
    }
    
    private async Task<T?> GetFromApiOrDefaultAsync<T>(string url) where T : class
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<T>(responseString);
        
        return result;
    }

    private async Task RefreshTokenAsync()
    {
        var tokenResponse = await SendRefreshAccessTokenRequestAsync(_stravaSettings.Value);
        _accessToken = tokenResponse?.AccessToken ?? string.Empty;
    }
    
    private static async Task<RefreshTokenResponse?> SendRefreshAccessTokenRequestAsync(StravaTokenSettings tokenSettings)
    {
        Dictionary<string, string> formData = new()
        {
            ["client_id"] = tokenSettings.ClientId,
            ["client_secret"] = tokenSettings.ClientSecret,
            ["refresh_token"] = tokenSettings.RefreshToken,
            ["grant_type"] = "refresh_token"
        };

        using var httpClient = new HttpClient();
        using var content = new FormUrlEncodedContent(formData);
        
        var response = await httpClient.PostAsync(StravaApi.RefreshTokenUrl, content);
        
        if (!response.IsSuccessStatusCode) return null;
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<RefreshTokenResponse>(responseContent);
    }
}