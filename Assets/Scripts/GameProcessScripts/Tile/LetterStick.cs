using Assets.GameProcessScripts;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterStick : MonoBehaviour, IDropHandler
{

    [SerializeField] FixedLetter fixedLetterPrefub;
    [SerializeField] Sprite yellowTileSprite;
    [SerializeField] Sprite redTileSprite;

    public ILetter Letter { get; set; }
    public int XCoord { get; set; }
    public int YCoord { get; set; }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (Letter != null && Letter is FixedLetter) return;

        if (eventData.pointerDrag != null)
        {
            HandLetter letter = eventData.pointerDrag.GetComponent<HandLetter>();

            letter.Stick(this);
        }
    }

    public void CreateLetter(OwnedLetterInfo letterInfo)
    {
        if (Letter is HandLetter handLetter)
        {
            handLetter.ResetToUI();
        }
        if (Letter is FixedLetter fixedLetter_)
        {
            Destroy(fixedLetter_.gameObject);
            Letter = null;
        }
        Vector3 tilePos = transform.position;
        tilePos.z -= 0.1f;
        FixedLetter fixedLetter = Instantiate<FixedLetter>(fixedLetterPrefub, tilePos, new Quaternion(0, 0, 0, 0), transform);
        fixedLetter.Stick(this);
        fixedLetter.ChangeInfo(letterInfo);
    }

    public void DeleteLetter()
    {
        if (Letter is FixedLetter fixedLetter_)
        {
            Destroy(fixedLetter_.gameObject);
            Letter = null;
        }
    }

    public void ChangeTileColor(bool toRed)
    {
        GetComponent<SpriteRenderer>().sprite = toRed ? redTileSprite : yellowTileSprite;
    }
}
