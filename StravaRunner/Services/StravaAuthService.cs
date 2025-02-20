using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StravaRunner.Constants;
using StravaRunner.Helpers;
using StravaRunner.Models;
using StravaRunner.Models.Settings;
using StravaRunner.Models.Strava;

namespace StravaRunner.Services;

public interface IStravaAuthService
{
    Task<string> GetApplicationAccessTokenAsync();
    Task<SecuredTokenWrapper<Athlete>> AuthorizeUser(string authorizationCode);
}

public class StravaAuthService(
    IOptions<StravaAuthSettings> stravaAuthSettings,
    IOptions<SecuritySettings> securitySettings)
    : IStravaAuthService
{
    private const string GrantTypeRefreshToken = "refresh_token";
    private const string GrantTypeAuthorizationCode = "authorization_code";
    private readonly StravaAuthSettings _stravaAuthSettings = stravaAuthSettings.Value;
    private readonly SecuritySettings _securitySettings = securitySettings.Value;
    private string _authToken = string.Empty;

    public async Task<string> GetApplicationAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_authToken)) return _authToken;
        
        var authResponse = await SendRefreshTokenRequestAsync(_stravaAuthSettings.RefreshToken);
        _authToken = authResponse.AccessToken;
        return _authToken;
    }
    
    public async Task<SecuredTokenWrapper<Athlete>> AuthorizeUser(string authorizationCode)
    {
        var tokenResponse = await SendAuthorizationCodeRequestAsync(authorizationCode);

        if (tokenResponse is null) return new SecuredTokenWrapper<Athlete>();
        
        var result = new SecuredTokenWrapper<Athlete>
        {
            Value = tokenResponse.Athlete,
            SecuredAccessToken = EncryptionHelper.Encrypt(tokenResponse.AccessToken, _securitySettings.EncryptTokenKey),
            SecuredRefreshToken = EncryptionHelper.Encrypt(tokenResponse.RefreshToken, _securitySettings.EncryptTokenKey),
            ExpiresAt = tokenResponse.ExpiresAt,
        };

        return result;
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
    
    private static async Task<RefreshTokenResponse?> SendOauthRequestAsync(IEnumerable<KeyValuePair<string, string>> formData)
    {
        using var httpClient = new HttpClient();
        using var content = new FormUrlEncodedContent(formData);
        
        var response = await httpClient.PostAsync(StravaApi.OauthRefreshUrl, content);
        
        if (!response.IsSuccessStatusCode) return null;
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<RefreshTokenResponse>(responseContent);
    }
}