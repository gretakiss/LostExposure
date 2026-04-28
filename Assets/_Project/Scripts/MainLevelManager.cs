using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLevelManager : MonoBehaviour
{
    [Header("Küldetés beállítások")]
    public int photosRequiredToWin = 3;
    public string hubSceneName = "NewHub";
    public string mainMenuName = "MainMenu";
    [Header("Tutorial")]
    public bool showTutorialOnStart = true;

    [Header("Referenciák")]
    public PhotoCameraSystem photoSystem;
    public EntityBrain ghostBrain;
    public GameObject exitDoor;

    [Header("UI elemek")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI interactPromptText;
    public TextMeshProUGUI threatLevelText;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    private bool isGameOver = false;
    private bool canLeaveLevel = false;
    private int lastPhotoCount = 0;
    private bool tutorialActive = false;

    private void Start()
    {
        // Scene induláskor visszaállítjuk az időt normál sebességre.
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        // Fontos: az exitDoor object NE legyen kikapcsolva,
        // mert különben a raycast nem tudja érzékelni.
        // Ha láthatatlan trigger cube-ot használsz, csak a Mesh Renderer legyen kikapcsolva.

        HidePrompt();
        UpdateObjectiveUI();
        if (showTutorialOnStart)
            StartCoroutine(ShowStartTutorial());
    }

    private void Update()
    {
        if (isGameOver)
            return;

        int currentPhotos = GetCurrentPhotoCount();

        // Ha változott a fotók száma, frissítjük a küldetés állapotát.
        if (currentPhotos != lastPhotoCount)
        {
            lastPhotoCount = currentPhotos;
            UpdateObjectiveUI();
            CheckMissionProgress(currentPhotos);
        }

        // Opcionális threat kijelzés debughoz vagy UI-hoz.
        if (threatLevelText != null && ghostBrain != null)
        {
            threatLevelText.text = "Threat: " + Mathf.RoundToInt(ghostBrain.threatLevel) + "%";
        }
    }
    public bool IsTutorialActive()
    {
        return tutorialActive;
    }
    // Pálya eleji tutorial
    private IEnumerator ShowStartTutorial()
    {
        tutorialActive = true;

        ShowPrompt("Press [F] to equip camera.");
        yield return new WaitForSeconds(4f);

        ShowPrompt("Right click to take a photo.");
        yield return new WaitForSeconds(4f);

        ShowPrompt("Capture 3 pieces of evidence.");
        yield return new WaitForSeconds(4f);

        tutorialActive = false;
        HidePrompt();
    }

    private int GetCurrentPhotoCount()
    {
        if (photoSystem == null)
            return 0;

        return photoSystem.GetPhotoCount();
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText == null)
            return;

        int current = GetCurrentPhotoCount();

        if (current < photosRequiredToWin)
            objectiveText.text = $"Collect evidence: {current} / {photosRequiredToWin} photos";
        else
            objectiveText.text = "Evidence collected. Return to the exit.";
    }

    private void CheckMissionProgress(int photos)
    {
        // 3 fotó után engedélyezzük a kijárat használatát.
        if (photos >= photosRequiredToWin && !canLeaveLevel)
        {
            canLeaveLevel = true;

            ShowPrompt("Objective complete. Return to the exit.");
            StartCoroutine(HidePromptAfterDelay(3f));

            Debug.Log("Mission objective complete. Exit is now available.");
        }
    }

    public bool CanLeaveLevel()
    {
        return canLeaveLevel;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("GAME OVER");

        // Kamera UI bezárása, hogy Game Over után ne lehessen F-fel kapcsolgatni.
        if (photoSystem != null)
            photoSystem.ForceCloseCameraUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Játék megállítása.
        Time.timeScale = 0f;

        // UI használatához kurzor feloldása.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void MissionSuccess()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        StartCoroutine(ReturnToHub());
    }

    private IEnumerator ReturnToHub()
    {
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;

        // Jelezzük a hub GameManagernek, hogy küldetésből térünk vissza.
        GameManager.returnedFromMission = true;

        SceneManager.LoadScene(hubSceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuName);
    }

    public void ShowPrompt(string message)
    {
        if (interactPromptText == null)
            return;

        interactPromptText.text = message;
        interactPromptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (interactPromptText == null)
            return;

        interactPromptText.text = "";
        interactPromptText.gameObject.SetActive(false);
    }

    private IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt();
    }
}