using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogOutUI : MonoBehaviour
{
    [SerializeField] public Button LogOutBtn;
    [SerializeField] public Button ShowStatsBtn;
    [SerializeField] private TextMeshProUGUI loggedUserText;

    private void Awake()
    {
        Hide();
    }

    public void SetLoggedUserText(string text)
    {
        loggedUserText.text = text;
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
