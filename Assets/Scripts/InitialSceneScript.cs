using System.Collections;
using UnityEngine;

public class InitialSceneScript : MonoBehaviour
{

    [SerializeField] float delayInSeconds;

    void Start()
    {
        StartCoroutine(LoadMainMenu(delayInSeconds));
    }

    IEnumerator LoadMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        Loader.Load(Loader.Scene.MainMenuScene);
    }
}
