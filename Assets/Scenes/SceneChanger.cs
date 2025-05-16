using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{

    [Header("UI ¼³Á¤")]
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GameObject howToPlay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public async void LoadGame()
    {
        // Apply fade out effect to Canvas
        while(canvas.scaleFactor > 0.01f)
        {
            canvas.scaleFactor *= 0.9f;
            await System.Threading.Tasks.Task.Delay(10);
        }

        // Load the game scene
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public async void LoadHowToPlay()
    {
        // Apply fade out effect to Canvas
        while (canvas.scaleFactor > 0.01f)
        {
            canvas.scaleFactor *= 0.9f;
            await System.Threading.Tasks.Task.Delay(10);
        }
        // Load the how to play scene
        howToPlay.SetActive(true);
    }
}
