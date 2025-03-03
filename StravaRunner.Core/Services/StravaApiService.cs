using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StravaRunner.Core.Constants;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models;
using StravaRunner.Core.Models.Settings;
using StravaRunner.Core.Models.Strava;

namespace StravaRunner.Core.Services;

public interface IStravaApiService
{
    Task<T?> GetFromUserApiAsync<T>(string url, string accessToken) where T : class;
    Task<T?> GetFromClientApiAsync<T>(string url) where T : class;
    Task<SecuredTokenWrapper<Athlete>> AuthorizeUser(string authorizationCode);
}

public class StravaApiService(
    IHttpClientFactory httpClientFactory,
    IEncryptionService encryptionService,
    IOptions<StravaAuthSettings> stravaAuthSettings) : IStravaApiService
{
    private const string GrantTypeRefreshToken = "refresh_token";
    private const string GrantTypeAuthorizationCode = "authorization_code";

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly StravaAuthSettings _stravaAuthSettings = stravaAuthSettings.Value;

    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiry;

    public async Task<T?> GetFromUserApiAsync<T>(string url, string accessToken) where T : class =>
        await SendGetRequestAsync<T>(url, accessToken);

    public async Task<T?> GetFromClientApiAsync<T>(string url)
        where T : class
    {
        var token = await GetOrRefreshTokenClientAsync();
        var response = await SendGetRequestAsync<T>(url, token);
        
        return response;
    }
    
    public async Task<SecuredTokenWrapper<Athlete>> AuthorizeUser(string authorizationCode)
    {
        var tokenResponse = await SendAuthorizationCodeRequestAsync(authorizationCode);

        if (tokenResponse is null) return new SecuredTokenWrapper<Athlete>();
        
        var result = new SecuredTokenWrapper<Athlete>
        {
            Value = tokenResponse.Athlete,
            SecuredAccessToken = _encryptionService.Encrypt(tokenResponse.AccessToken),
            SecuredRefreshToken = _encryptionService.Encrypt(tokenResponse.RefreshToken),
            ExpiresAt = tokenResponse.ExpiresAt,
        };

        return result;
    }

    private async Task<string> GetOrRefreshTokenClientAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiry > DateTimeOffset.UtcNow)
            return _accessToken;
        
        var authResponse = await SendRefreshTokenRequestAsync(_stravaAuthSettings.RefreshToken);
        _accessToken = authResponse!.AccessToken;
        _accessTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(authResponse.ExpiresAt);
        return _accessToken;
    }
    
    private async Task<RefreshTokenResponse?> SendAuthorizationCodeRequestAsync(string authorizationCode)
    {
        Dictionary<string, string> formData = new()
        {
            ["client_id"] = _stravaAuthSettings.ClientId,
            ["client_secret"] = _stravaAuthSettings.ClientSecret,
            ["code"] = authorizationCode,
            ["grant_type"] = GrantTypeAuthorizationCode
        };
        
        return await SendOauthRequestAsync(formData);
    }

    private async Task<RefreshTokenResponse?> SendRefreshTokenRequestAsync(string refreshToken)
    {
        Dictionary<string, string> formData = new()
        {
            ["client_id"] = _stravaAuthSettings.ClientId,
            ["client_secret"] = _stravaAuthSettings.ClientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = GrantTypeRefreshToken
        };
        
        return await SendOauthRequestAsync(formData);
    }
    
    private async Task<T?> SendGetRequestAsync<T>(string url, string token) where T : class
    {
        using var client = _httpClientFactory.CreateClient(nameof(StravaApiService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<T>(responseString);
        
        return result;
    }
    
    private async Task<RefreshTokenResponse?> SendOauthRequestAsync(IEnumerable<KeyValuePair<string, string>> formData)
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(StravaApiService));
        using var content = new FormUrlEncodedContent(formData);
        
        var response = await httpClient.PostAsync(StravaApi.OauthRefreshUrl, content);
        
        if (!response.IsSuccessStatusCode) return null;
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<RefreshTokenResponse>(responseContent);
    }
}