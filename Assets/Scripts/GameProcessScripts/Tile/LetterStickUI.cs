using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterStickUI : MonoBehaviour, IDropHandler
{

    public HandLetter Letter { get; set; }

    public void OnDrop(PointerEventData eventData)
    {
        HandLetter letter = eventData.pointerDrag.GetComponent<HandLetter>();

        if (Letter != null && Letter != letter) return;

        if (eventData.pointerDrag != null)
        {
            letter.StickUI(this);
        }
    }
}
