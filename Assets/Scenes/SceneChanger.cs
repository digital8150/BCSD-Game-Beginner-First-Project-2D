using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;
public class SceneChanger : MonoBehaviour
{

    [Header("UI ¼³Á¤")]
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject howToPlay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        canvas.SetActive(true);
        howToPlay.SetActive(false);
    }
    public async void LoadGame()
    {
        // Apply fade out effect to Canvas
        await fadeCanvas();

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
        await fadeCanvas();
        // Load the how to play scene
        howToPlay.SetActive(true);
    }

    public void ReturnToMenu()
    {
        canvas.GetComponent<Canvas>().scaleFactor = 1.0f;
        howToPlay.SetActive(false);
        canvas.SetActive(true);
    }

    async Task fadeCanvas()
    {
        while (canvas.GetComponent<Canvas>().scaleFactor > 0.01f)
        {
            canvas.GetComponent<Canvas>().scaleFactor *= 0.9f;
            await System.Threading.Tasks.Task.Delay(10);
        }
         
    }
}
