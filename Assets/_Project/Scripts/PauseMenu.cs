using UnityEngine;
using UnityEngine.SceneManagement;

// Ez a script kezeli a játék pause menüjét.
// Escape gombbal megállítható és folytatható a játék.
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // A pause panel alapból ne látszódjon.
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Escape gombra váltunk pause és normál állapot között.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // Pause UI megjelenítése.
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // A játék megállítása.
        Time.timeScale = 0f;

        // Menü használatához a kurzort feloldjuk.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Pause UI eltüntetése.
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Játék folytatása.
        Time.timeScale = 1f;

        // FPS nézethez visszazárjuk a kurzort.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMainMenu()
    {
        // Scene váltás elõtt visszaállítjuk az idõt.
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}