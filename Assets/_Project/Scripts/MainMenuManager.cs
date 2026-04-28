using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string firstSceneName = "NewHub";

    [Header("UI")]
    public TextMeshProUGUI loadMessageText;

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstSceneName);
    }

    public void LoadGame()
    {
        if (loadMessageText != null)
        {
            loadMessageText.text = "Load Game is currently unavailable. Save system in development.";
            StartCoroutine(HideLoadMessage());
        }
    }

    private IEnumerator HideLoadMessage()
    {
        yield return new WaitForSeconds(4f);

        if (loadMessageText != null)
            loadMessageText.text = "";
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed.");
        Application.Quit();
    }
}