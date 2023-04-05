using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayersManager.Instance.ChangeColor(colorId);
        });
    }

    private void Start()
    {
        PlayersManager.Instance.OnPlayerDataListChanged += PlayersManager_OnPlayerDataListChanged;

        image.color = PlayersManager.Instance.GetColor(colorId);

        UpdateIsSelected();
    }


    private void OnDestroy()
    {
        PlayersManager.Instance.OnPlayerDataListChanged -= PlayersManager_OnPlayerDataListChanged;
    }

    private void PlayersManager_OnPlayerDataListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (PlayersManager.Instance.GetPlayerData().colorId == colorId) {
            GetComponent<RectTransform>().localScale = new Vector3(1.1f, 1.1f, 1);
        }
        else
        {
            GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }
}
