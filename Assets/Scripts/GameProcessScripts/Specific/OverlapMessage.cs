using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GameProcessScripts
{
    internal class OverlapMessage: MonoBehaviour
    {

        [SerializeField] TextMeshProUGUI textMeshPro;
        [SerializeField] Button mainMenuBtn;

        private void Start()
        {
            mainMenuBtn.onClick.AddListener(() => {
                Loader.Load(Loader.Scene.MainMenuScene);
            });
        }

        public void SetPopUpText(string text)
        {
            textMeshPro.text = text;
        }

        public void Show(string text)
        {
            SetPopUpText(text);
            gameObject.SetActive(true);
        }

        public void Hide() 
        {
            gameObject.SetActive(false);
        }
    }
}
