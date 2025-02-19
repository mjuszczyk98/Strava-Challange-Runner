using Newtonsoft.Json;

namespace StravaRunner.Models;

public class Athlete
{
    [JsonProperty("resource_state")]
    public int ResourceState { get; set; }
    [JsonProperty("firstname")]
    public string? FirstName { get; set; }
    [JsonProperty("lastname")]
    public string? LastName { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}