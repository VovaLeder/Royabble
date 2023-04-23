using Newtonsoft.Json;

public class UserChangeRequestData
{
    [JsonProperty("auth_key")]
    public string authKey;

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

    public UserChangeRequestData()
    {
        authKey = "";
        gamesPlayed = 0;
        gamesWon = 0;
        earnedPoints = 0;
        spentPoints = 0;
        wordsComposed = 0;
        playersKilled = 0;
    }

    public UserChangeRequestData(string authKey, int gamesPlayed, int gamesWon, int earnedPoints, int spentPoints, int wordsComposed, int playersKilled)
    {
        this.authKey = authKey;
        this.gamesPlayed = gamesPlayed;
        this.gamesWon = gamesWon;
        this.earnedPoints = earnedPoints;
        this.spentPoints = spentPoints;
        this.wordsComposed = wordsComposed;
        this.playersKilled = playersKilled;
    }
}