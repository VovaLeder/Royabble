using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoreBtn : MonoBehaviour
{
    [SerializeField] Button btn;
    [SerializeField] TextMeshProUGUI price;

    public void SetPrice(int price)
    {
        this.price.text = price.ToString();
    }

    public void SetOnClickListener(UnityAction call)
    {
        btn.onClick.AddListener(call);
    }

    public void SetActive(bool active)
    {
        btn.interactable = active;
    }
}
