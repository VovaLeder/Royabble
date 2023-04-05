using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{

    [SerializeField] Button closeBtn;
    [SerializeField] Button createPublicBtn;
    [SerializeField] Button createPrivateBtn;
    [SerializeField] TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        createPublicBtn.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(lobbyNameInputField.text, false);
        });
        createPrivateBtn.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(lobbyNameInputField.text, true);
        }); 
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
