using Newtonsoft.Json;

public class UserData
{
    [JsonProperty("auth_key")]
    public string authKey;

    [JsonProperty("username")]
    public string username;

    [JsonProperty("games_played")]
    public int gamesPlayed;
    [JsonProperty("games_won")]
    public int gamesWon;

    [JsonProperty("earned_points")]
    public int earnedPoints;
    [JsonProperty("spent_points")]
    public int spentPoints;
    [JsonProperty("words_composed")]
    public int wordsComposed;
    [JsonProperty("players_killed")]
    public int playersKilled;
}