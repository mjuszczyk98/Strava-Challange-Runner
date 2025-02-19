using Newtonsoft.Json;

namespace StravaRunner.Models.Strava;

public class Activity
{
    [JsonProperty("resource_state")]
    public int ResourceState { get; set; }
    [JsonProperty("athlete")]
    public Athlete? Athlete { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("distance")]
    public double Distance { get; set; }
    [JsonProperty("moving_time")]
    public double MovingTime { get; set; }
    [JsonProperty("elapsed_time")]
    public double ElapsedTime { get; set; }
    [JsonProperty("total_elevation_gain")]
    public double TotalElevationGain { get; set; }
    [JsonProperty("type")]
    public ActivityType Type { get; set; } = ActivityType.None;
    [JsonProperty("sport_type")]
    public SportType SportType { get; set; } = SportType.None;
}