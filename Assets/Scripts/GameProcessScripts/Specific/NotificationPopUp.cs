using TMPro;
using UnityEngine;

namespace Assets.GameProcessScripts
{
    internal class NotificationPopUp: MonoBehaviour
    {

        [SerializeField] TextMeshProUGUI textMeshPro;

        public void SetPopUpText(string text)
        {
            textMeshPro.text = text;
        }
    }
}
