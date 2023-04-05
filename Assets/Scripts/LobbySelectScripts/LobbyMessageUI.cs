using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{


    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;


    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        PlayersManager.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
        LobbyManager.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFailed;
        LobbyManager.Instance.OnQuickJoinFailed += KitchenGameLobby_OnQuickJoinFailed;

        Hide();
    }

    private void OnDestroy()
    {
        PlayersManager.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
        LobbyManager.Instance.OnCreateLobbyStarted -= KitchenGameLobby_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed -= KitchenGameLobby_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinStarted -= KitchenGameLobby_OnJoinStarted;
        LobbyManager.Instance.OnJoinFailed -= KitchenGameLobby_OnJoinFailed;
        LobbyManager.Instance.OnQuickJoinFailed -= KitchenGameLobby_OnQuickJoinFailed;
    }

    private void KitchenGameLobby_OnQuickJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Не удалось найти лобби с помощью быстрого поиска.");
    }

    private void KitchenGameLobby_OnJoinFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Не удалось присоединиться к лобби.");
    }

    private void KitchenGameLobby_OnJoinStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Присоединяемся к лобби ...");
    }

    private void KitchenGameLobby_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Не удалось создать лобби.");
    }

    private void KitchenGameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Создаем лобби ...");
    }

    private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
        {
            ShowMessage("Не удалось присоединиться.");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
