using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Beállítások")]
    public float interactDistance = 3f;
    public Camera playerCamera;

    [Header("Hub scene")]
    public GameManager gameManager;

    [Header("Main level scene")]
    public MainLevelManager mainLevelManager;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (mainLevelManager == null)
            mainLevelManager = FindFirstObjectByType<MainLevelManager>();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        // Game Over alatt ne lehessen interaktálni.
        if (mainLevelManager != null && mainLevelManager.IsGameOver())
            return;

        // Hub scene-ben csak akkor engedünk interakciót, ha már elindult a játék.
        if (gameManager != null)
        {
            if (!gameManager.HasGameStarted())
            {
                gameManager.HideNotification();
                return;
            }

            // Email vagy evidence galéria közben ne legyen raycast interakció.
            if (gameManager.IsEmailOpen() || gameManager.IsEvidenceGalleryOpen())
                return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool foundInteractable = false;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Monitor") && gameManager != null)
            {
                foundInteractable = true;

                if (MissionPhotoArchive.Count() > 0)
                    gameManager.ShowNotification("[E] View Evidence");
                else
                    gameManager.ShowNotification("[E] Check Monitor");

                if (Input.GetKeyDown(KeyCode.E))
                    gameManager.OpenMonitor();
            }
            else if (hit.collider.CompareTag("Door") && gameManager != null)
            {
                foundInteractable = true;

                // Ha már vannak fotók → mission report flow
                if (MissionPhotoArchive.Count() > 0)
                {
                    if (gameManager.HasEvidenceBeenViewed())
                    {
                        gameManager.ShowNotification("[E] View Mission Report");

                        if (Input.GetKeyDown(KeyCode.E))
                            gameManager.OpenMissionSummary();
                    }
                    else
                    {
                        gameManager.ShowNotification("Review your evidence on the monitor first.");
                    }
                }
                // Normál flow (küldetés indulás)
                else if (gameManager.missionAccepted)
                {
                    gameManager.ShowNotification("[E] Leave Office");

                    if (Input.GetKeyDown(KeyCode.E))
                        gameManager.LoadNextLevel();
                }
                else
                {
                    gameManager.ShowNotification("Check your email first.");
                }
            }
            else if (hit.collider.CompareTag("ReturnDoor") && mainLevelManager != null)
            {
                foundInteractable = true;

                if (mainLevelManager.CanLeaveLevel())
                {
                    mainLevelManager.ShowPrompt("[E] Return to office");

                    if (Input.GetKeyDown(KeyCode.E))
                        mainLevelManager.MissionSuccess();
                }
                else
                {
                    mainLevelManager.ShowPrompt("Capture more evidence first.");
                }
            }
        }

        if (!foundInteractable)
        {
            if (gameManager != null && gameManager.CanHideNotification())
                gameManager.HideNotification();

            if (mainLevelManager != null && !mainLevelManager.IsTutorialActive())
                mainLevelManager.HidePrompt();
        }
    }
}