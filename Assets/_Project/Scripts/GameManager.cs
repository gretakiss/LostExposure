using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Ezt a MainLevelManager állítja true-ra, amikor a játékos visszatér a fõ pályáról a hubba.
    // Így a hub scene újratöltésekor nem indul újra az intro.
    public static bool returnedFromMission = false;

    [Header("UI elemek")]
    public GameObject introBackground;
    public GameObject emailPanel;
    public GameObject loadingScreen;
    public TextMeshProUGUI notificationText;

    [Header("Evidence Gallery")]
    public GameObject evidenceGalleryPanel;          // A monitoron megnyíló bizonyíték galéria panel
    public RawImage evidenceImage;                   // Az aktuális fotó megjelenítésére szolgáló RawImage
    public TextMeshProUGUI evidenceCounterText;      // Galéria számláló, például: 1 / 3

    [Header("Mission Summary")]
    public GameObject missionSummaryPanel;           // A küldetés végén megjelenõ összegzõ panel
    public TextMeshProUGUI missionSummaryText;       // Ide kerül a fotók száma, entity fotók száma, fizetés
    public int paymentPerEntityPhoto = 150;          // Ennyi pénzt kap a játékos entity-t tartalmazó fotónként

    [Header("Hang")]
    public AudioSource computerAudio;

    [Header("Játékos")]
    public MonoBehaviour fpsController;

    [Header("Scene váltás")]
    public string nextSceneName = "Corridor";

    [Header("Küldetés")]
    public bool missionAccepted = false;

    private bool isGameStarted = false;
    private bool evidenceViewed = false;             // Jelzi, hogy a játékos megnézte-e már a visszahozott fotókat
    private float notificationBlockUntil = 0f;
    private int currentEvidenceIndex = 0;

    private void Start()
    {
        // Biztonsági visszaállítás, ha másik scene-bõl vagy pause/game over állapotból érkezünk.
        Time.timeScale = 1f;

        // A bizonyíték galéria alapból ne látszódjon.
        if (evidenceGalleryPanel != null)
            evidenceGalleryPanel.SetActive(false);

        // A mission summary panel alapból ne látszódjon.
        if (missionSummaryPanel != null)
            missionSummaryPanel.SetActive(false);

        // Ha küldetésbõl tértünk vissza, kihagyjuk az intro képernyõt.
        if (returnedFromMission)
        {
            returnedFromMission = false;

            isGameStarted = true;
            missionAccepted = false;

            if (introBackground != null) introBackground.SetActive(false);
            if (emailPanel != null) emailPanel.SetActive(false);
            if (loadingScreen != null) loadingScreen.SetActive(false);

            HideNotification();

            if (fpsController != null)
                fpsController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ShowNotification("You returned to the office.");
            StartCoroutine(HideNotificationAfterDelay(3f));

            return;
        }

        // Normál elsõ indulás esetén az intro háttér látszik.
        if (introBackground != null) introBackground.SetActive(true);
        if (emailPanel != null) emailPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(false);

        HideNotification();

        // Intro alatt a játékos ne tudjon mozogni.
        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Space lenyomására indul el a hub jelenet.
        if (!isGameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }

        // Ha a galéria nyitva van, billentyûkkel is lehessen lapozni vagy bezárni.
        if (evidenceGalleryPanel != null && evidenceGalleryPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                NextEvidencePhoto();

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                PreviousEvidencePhoto();

            if (Input.GetKeyDown(KeyCode.Escape))
                CloseEvidenceGallery();
        }

        // Ha a mission summary panel nyitva van, ESC-re zárható.
        if (missionSummaryPanel != null && missionSummaryPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseMissionSummary();
        }
    }

    private void StartGame()
    {
        isGameStarted = true;

        if (introBackground != null)
            introBackground.SetActive(false);

        // Email érkezés hangja.
        if (computerAudio != null)
            computerAudio.Play();

        ShowNotification("You have received a new email.");

        // Pár másodpercig nem engedjük, hogy az interakciós rendszer eltüntesse az értesítést.
        notificationBlockUntil = Time.time + 3f;
        StartCoroutine(HideNotificationAfterDelay(6f));

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenMonitor()
    {
        // Ha már vannak küldetésbõl hozott fotók, akkor a monitor a galériát nyitja meg.
        // Ha még nincs bizonyíték, akkor az email panelt.
        if (MissionPhotoArchive.Count() > 0)
        {
            OpenEvidenceGallery();
        }
        else
        {
            OpenEmail();
        }
    }

    public void OpenEmail()
    {
        if (emailPanel != null)
            emailPanel.SetActive(true);

        HideNotification();

        // Email olvasása közben a mozgást letiltjuk.
        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void AcceptMission()
    {
        // A játékos elfogadta a küldetést, innentõl elhagyhatja az irodát.
        missionAccepted = true;

        if (emailPanel != null)
            emailPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ShowNotification("Objective updated.");
        StartCoroutine(HideNotificationAfterDelay(2f));
    }

    public void LoadNextLevel()
    {
        // Küldetés elfogadása nélkül ne lehessen pályát váltani.
        if (!missionAccepted)
            return;

        StartCoroutine(TransitionToNextLevel());
    }

    private IEnumerator TransitionToNextLevel()
    {
        if (fpsController != null)
            fpsController.enabled = false;

        HideNotification();

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(nextSceneName);
    }

    public void OpenEvidenceGallery()
    {
        // Ha nincs mit megjeleníteni, csak rövid üzenetet adunk.
        if (MissionPhotoArchive.Count() == 0)
        {
            ShowNotification("No evidence available.");
            StartCoroutine(HideNotificationAfterDelay(2f));
            return;
        }

        if (evidenceGalleryPanel != null)
            evidenceGalleryPanel.SetActive(true);

        // Ha a játékos megnyitotta a képeket, akkor késõbb az ajtónál elérhetõ lesz a mission report.
        evidenceViewed = true;

        // Galéria megnyitásakor az elsõ képpel kezdünk.
        currentEvidenceIndex = 0;
        ShowEvidencePhoto();

        HideNotification();

        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseEvidenceGallery()
    {
        if (evidenceGalleryPanel != null)
            evidenceGalleryPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void NextEvidencePhoto()
    {
        if (MissionPhotoArchive.Count() == 0)
            return;

        currentEvidenceIndex++;

        if (currentEvidenceIndex >= MissionPhotoArchive.Count())
            currentEvidenceIndex = 0;

        ShowEvidencePhoto();
    }

    public void PreviousEvidencePhoto()
    {
        if (MissionPhotoArchive.Count() == 0)
            return;

        currentEvidenceIndex--;

        if (currentEvidenceIndex < 0)
            currentEvidenceIndex = MissionPhotoArchive.Count() - 1;

        ShowEvidencePhoto();
    }

    private void ShowEvidencePhoto()
    {
        // Az aktuális fotó lekérése a scene váltások között is megmaradó archívumból.
        Texture2D photo = MissionPhotoArchive.GetPhoto(currentEvidenceIndex);

        if (evidenceImage != null)
        {
            evidenceImage.texture = photo;
            evidenceImage.color = Color.white;
        }

        if (evidenceCounterText != null)
            evidenceCounterText.text = (currentEvidenceIndex + 1) + " / " + MissionPhotoArchive.Count();
    }

    public bool HasEvidenceBeenViewed()
    {
        return evidenceViewed;
    }

    public void OpenMissionSummary()
    {
        if (missionSummaryPanel != null)
            missionSummaryPanel.SetActive(true);

        int totalPhotos = MissionPhotoArchive.Count();
        int entityPhotos = MissionPhotoArchive.EntityPhotoCount();
        int payment = entityPhotos * paymentPerEntityPhoto;

        if (missionSummaryText != null)
        {
            missionSummaryText.text =
                "MISSION REPORT\n\n" +
                "Photos captured: " + totalPhotos + "\n" +
                "Entity evidence: " + entityPhotos + "\n\n" +
                "Payment received: $" + payment + "\n\n" +
                "Further assignments are currently unavailable.";
        }

        HideNotification();

        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMissionSummary()
    {
        if (missionSummaryPanel != null)
            missionSummaryPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void NextLevelUnderDevelopment()
    {
        ShowNotification("Next level is currently under development.");
        StartCoroutine(HideNotificationAfterDelay(3f));
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Quit Game pressed.");
        Application.Quit();
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null) return;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
    }

    public void HideNotification()
    {
        if (notificationText == null) return;

        notificationText.text = "";
        notificationText.gameObject.SetActive(false);
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotification();
    }

    public bool CanHideNotification()
    {
        return Time.time >= notificationBlockUntil;
    }

    public bool HasGameStarted()
    {
        return isGameStarted;
    }

    public bool IsEmailOpen()
    {
        return emailPanel != null && emailPanel.activeSelf;
    }

    public bool IsEvidenceGalleryOpen()
    {
        return evidenceGalleryPanel != null && evidenceGalleryPanel.activeSelf;
    }

    public bool IsMissionSummaryOpen()
    {
        return missionSummaryPanel != null && missionSummaryPanel.activeSelf;
    }
}