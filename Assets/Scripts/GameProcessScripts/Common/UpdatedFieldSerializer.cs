using Assets.GameProcessScripts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.GameProcessScripts
{
 
    // Скорее всего уже использовать не буду, раз есть Json Serializer, но это было написано мной 31.01.2023
    public class UpdatedFieldSerializer
    {
        static public string Serialize(NetLetterInfoCollection updatedField)
        {
            StringBuilder BobTheBuilder = new();

            foreach (var updatedTile in updatedField)
            {
                BobTheBuilder.Append(JsonConvert.SerializeObject(updatedTile));
            }

            return BobTheBuilder.ToString();
        }

        static public Dictionary<(int i, int j), string> Deserialize(string updatedField)
        {
            Dictionary<(int i, int j), string> updatedFieldObj = new();

            Regex updatedTileRegex = new Regex(@"(\[\(\d+, \d+\), \w*\])");
            MatchCollection updatedTileMathces = updatedTileRegex.Matches(updatedField);

            foreach (Match updatedTileMatch in updatedTileMathces)
            {

                string updatedTileString = updatedTileMatch.Value;

                Regex iRegex = new Regex(@"(\(\d*,)");
                Regex jRegex = new Regex(@"( \d*\))");
                Regex valueRegex = new Regex(@"( \w*\])");

                Match iMatch = iRegex.Match(updatedTileString);
                Match jMatch = jRegex.Match(updatedTileString);
                Match valueMatch = valueRegex.Match(updatedTileString);

                int i = int.Parse(iMatch.Value.Substring(1, iMatch.Value.Length - 2));
                int j = int.Parse(jMatch.Value.Substring(1, jMatch.Value.Length - 2));
                string value = valueMatch.Value.Substring(1, valueMatch.Value.Length - 2);

                updatedFieldObj.Add((i, j), value);
            }


            return updatedFieldObj;
        }
    }
}
