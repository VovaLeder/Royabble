using Newtonsoft.Json;

public class UserResponceData
{
    [JsonProperty("message")]
    public string message;

    [JsonProperty("user")]
    public UserData user;
}