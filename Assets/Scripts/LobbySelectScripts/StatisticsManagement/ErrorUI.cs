using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class ErrorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;

    private void Start()
    {
        Hide();
    }

    public void ShowErrorText(string error)
    {
        errorText.text = error;
        Show();
        StartCoroutine(HideAfterSeconds(3));
    }

    public void Show()
    {
        //TODO first time does not work
        gameObject.SetActive(true);
    }

    private IEnumerator HideAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
