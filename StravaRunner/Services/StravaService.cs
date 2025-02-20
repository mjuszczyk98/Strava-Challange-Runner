using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using StravaRunner.Constants;
using StravaRunner.Models.Settings;
using StravaRunner.Models.Strava;

namespace StravaRunner.Services;

public interface IStravaService
{
    Task<List<Activity>> GetActivitiesAsync(string clubId);
}

public class StravaService(IStravaAuthService stravaAuthService, IMongoDatabase db) : IStravaService
{
    private readonly IStravaAuthService _stravaAuthService = stravaAuthService;

    public async Task<List<Activity>> GetActivitiesAsync(string clubId)
    {
        var activities = await GetFromApiWithRetryAsync<List<Activity>>(StravaApi.GetClubUrl(clubId));
        
        var col = db.GetCollection<Activity>("Activities");
        col.InsertMany(activities);
        
        return activities;
    }

    private async Task<T?> GetFromApiWithRetryAsync<T>(string url)
        where T : class
    {
        var token = await _stravaAuthService.GetApplicationAccessTokenAsync();
        var response = await GetFromApiOrDefaultAsync<T>(url, token);
        
        return response;
    }
    
    private async Task<T?> GetFromApiOrDefaultAsync<T>(string url, string token) where T : class
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<T>(responseString);
        
        return result;
    }
}