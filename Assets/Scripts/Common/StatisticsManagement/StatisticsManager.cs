using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using Unity.Netcode;

public class StatisticsManager : NetworkBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    const string PLAYER_PREFS_AUTH_KEY = "AuthKey";

    public UserData currentUser { get; private set; }
    public UserChangeRequestData userChangeRequestData;

    public bool networkUnavailable;
    public bool success;
    public string errorMessage;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        currentUser = new();
        userChangeRequestData = new();
        currentUser.authKey = PlayerPrefs.GetString(PLAYER_PREFS_AUTH_KEY, "");
    }


    [ClientRpc]
    public void AddGameToCurrentUserClientRpc(ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.gamesPlayed++;
    }

    [ClientRpc]
    public void AddWonGameToCurrentUserClientRpc(ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.gamesWon++;
    }

    [ClientRpc]
    public void AddEarnedPointsToCurrentUserClientRpc(int earnedPoints, ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.earnedPoints += earnedPoints;
    }

    [ClientRpc]
    public void AddSpentPointsToCurrentUserClientRpc(int spentPoints, ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.spentPoints += spentPoints;
    }

    [ClientRpc]
    public void AddComposedWordToCurrentUserClientRpc(ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.wordsComposed++;
    }

    [ClientRpc]
    public void AddKilledPlayerToCurrentUserClientRpc(ClientRpcParams clientRpcParams = default)
    {
        userChangeRequestData.playersKilled++;
    }


    public bool LoggedIn()
    {
        return !currentUser.authKey.Equals("");
    }

    public IEnumerator UpdateUser()
    {
        if (LoggedIn())
        {
            userChangeRequestData.authKey = currentUser.authKey;
            yield return StartCoroutine(ChangeUser(userChangeRequestData));
            yield return StartCoroutine(LogIn(currentUser.authKey));
        }
        userChangeRequestData = new();
    }

    public void ResetUser()
    {
        currentUser = new();
        PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, "");
    }

    public IEnumerator LogIn(string authToken)
    {
        var user = new UserRequestAuthData();
        user.authToken = authToken;

        string json = JsonConvert.SerializeObject(user);

        var req = new UnityWebRequest("localhost:5000/api/get_user_by_auth", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(jsonToSend);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            networkUnavailable = true;
            Debug.Log("Error While Sending: " + req.error);
        }
        else
        {
            networkUnavailable = false;
            if (req.responseCode == 200)
            {
                success = true;
                UserResponceData userResponceData = JsonConvert.DeserializeObject<UserResponceData>(req.downloadHandler.text);
                currentUser = userResponceData.user;
                Debug.Log($"Received: {currentUser.username}");
            }
            else {
                success = false;
                errorMessage = JsonConvert.DeserializeObject<ErrorMessageData>(req.downloadHandler.text).message;

                currentUser.authKey = "";
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, "");
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator LogIn(string username, string password)
    {
        var user = new UserRequestUsernamePasswordData();
        user.username = username;
        user.password = password;

        string json = JsonConvert.SerializeObject(user);

        var req = new UnityWebRequest("localhost:5000/api/get_user", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(jsonToSend);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            networkUnavailable = true;
            Debug.Log("Error While Sending: " + req.error);
        }
        else
        {
            networkUnavailable = false;
            if (req.responseCode == 200)
            {
                success = true;
                UserResponceData userResponceData = JsonConvert.DeserializeObject<UserResponceData>(req.downloadHandler.text);
                currentUser = userResponceData.user;
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, currentUser.authKey);
                Debug.Log($"Received: {currentUser.username}");
            }
            else 
            {
                success = false;
                errorMessage = JsonConvert.DeserializeObject<ErrorMessageData>(req.downloadHandler.text).message;

                currentUser.authKey = "";
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, "");
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator CreateUser(string username, string password)
    {
        var user = new UserRequestUsernamePasswordData();
        user.username = username;
        user.password = password;

        string json = JsonConvert.SerializeObject(user);

        var req = new UnityWebRequest("localhost:5000/api/create_user", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(jsonToSend);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            networkUnavailable = true;
            Debug.Log("Error While Sending: " + req.error);
        }
        else
        {
            networkUnavailable = false;
            if (req.responseCode == 201)
            {
                success = true;
                UserResponceData userResponceData = JsonConvert.DeserializeObject<UserResponceData>(req.downloadHandler.text);
                currentUser = userResponceData.user;
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, currentUser.authKey);
                Debug.Log($"Received: {currentUser.username}");
            }
            else 
            {
                success = false;
                errorMessage = JsonConvert.DeserializeObject<ErrorMessageData>(req.downloadHandler.text).message;
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public IEnumerator ChangeUser(UserChangeRequestData user)
    {
        string json = JsonConvert.SerializeObject(user);

        var req = new UnityWebRequest("localhost:5000/api/change_user", "PUT");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(jsonToSend);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return req.SendWebRequest();

        if (req.isNetworkError)
        {
            networkUnavailable = true;
            Debug.Log("Error While Sending: " + req.error);
        }
        else
        {
            networkUnavailable = false;
            if (req.responseCode == 201)
            {
                success = true;
                UserResponceData userResponceData = JsonConvert.DeserializeObject<UserResponceData>(req.downloadHandler.text);
                currentUser = userResponceData.user;
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_KEY, currentUser.authKey);
                Debug.Log($"Received: {currentUser.username}");
            }
            else
            {
                success = false;
                errorMessage = JsonConvert.DeserializeObject<ErrorMessageData>(req.downloadHandler.text).message;
            }
        }

        yield return new WaitForEndOfFrame();
    }
}
