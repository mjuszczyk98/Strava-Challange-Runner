using Newtonsoft.Json;

namespace StravaRunner.Core.Models.Strava;

public class Athlete
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("firstname")]
    public string? FirstName { get; set; }
    [JsonProperty("lastname")]
    public string? LastName { get; set; }
    [JsonProperty("city")]
    public string? City { get; set; }
    [JsonProperty("country")]
    public string? Country { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}