using Assets.GameProcessScripts;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandLetter : MonoBehaviour, ILetter, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public LetterStickUI TileUI;
    public LetterStick Tile;

    public string Value;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        rectTransform.anchoredPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        GetComponent<SpriteRenderer>().forceRenderingOff = true;
        rectTransform.SetParent(UIManager.Instance.DragLetterUI.transform);
        GetComponent<RectTransform>().localScale = Vector3.one;
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchoredPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ResetPosition();
    }

    public void Stick(LetterStick tile)
    {
        if (Tile != null) Tile.Letter = null;
        Tile = tile;
        tile.Letter = this;

        GetComponent<SpriteRenderer>().forceRenderingOff = false;
        GetComponent<RectTransform>().SetParent(Tile.GetComponent<Transform>().parent);
        GetComponent<RectTransform>().position = Tile.GetComponent<Transform>().position;
        GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void ResetPosition()
    {
        if (Tile != null)
        {
            Stick(Tile);
        }
        else
        {
            ResetToUI();
        }
    }

    public void ResetToUI()
    {
        if (Tile != null)
        {
            Tile.Letter = null;
            Tile = null;
        }

        rectTransform.SetParent(UIManager.Instance.HandLetterUI.transform);

        rectTransform.anchorMax = new Vector2(0.5f, 0);
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchoredPosition = TileUI.GetComponent<RectTransform>().anchoredPosition;
        GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void ChangeValue(string value)
    {
        Value = value;

        Sprite sprite = Resources.Load(Path.Combine("Sprites", "Letters", "Russian", value), typeof(Sprite)) as Sprite;

        GetComponent<SpriteRenderer>().sprite = sprite;
        GetComponent<Image>().sprite = sprite;
    }

    public string GetValue()
    {
        return Value;
    }

}
