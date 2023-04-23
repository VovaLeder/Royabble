using Newtonsoft.Json;

public class UserRequestAuthData
{
    [JsonProperty("auth_key")]
    public string authToken;
}