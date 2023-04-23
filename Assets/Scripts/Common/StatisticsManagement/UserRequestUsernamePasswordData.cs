using Newtonsoft.Json;

public class UserRequestUsernamePasswordData
{
    [JsonProperty("username")]
    public string username;

    [JsonProperty("password")]
    public string password;
}