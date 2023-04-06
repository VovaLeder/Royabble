using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterStickUI : MonoBehaviour, IDropHandler
{

    private HandLetter Letter;

    public HandLetter GetLetter()
    {
        return Letter;
    }

    public void SetLetter(HandLetter letter)
    {
        Letter = letter;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {   
            HandLetter letter = eventData.pointerDrag.GetComponent<HandLetter>();

            if (Letter != null && Letter != letter) return;

            AssignLetter(letter);
        }
    }

    public void StickLetterBack()
    {
        if (Letter != null)
        {
            Letter.ResetToUI();
        }
    }

    public void AssignLetter(HandLetter letter)
    {
        if (Letter == null) {
            if (letter.TileUI != null)
            {
                letter.TileUI.UnassignLetter();
            }
            Letter = letter;
            Letter.TileUI = this;
            StickLetterBack();
        }
    }
    
    private void UnassignLetter()
    {
        Letter = null;
    }
}
