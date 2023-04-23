using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInUI : MonoBehaviour
{
    [SerializeField] public Button LogInBtn;
    [SerializeField] public Button CreateUserBtn;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;

    public string GetUsername()
    {
        return usernameField.text;
    }

    public string GetPassword()
    {
        return passwordField.text;
    }

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
