using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Ez a script kezeli:
// - kamera elővétel (F)
// - fotózás (jobb klikk)
// - galéria (TAB)
// - fotók mentése és archiválása
public class PhotoCameraSystem : MonoBehaviour
{
    [Header("Kamera")]
    public Camera photoCamera;
    public RenderTexture photoRenderTexture;
    public Light flashLight;

    [Header("UI")]
    public GameObject cameraOverlay;
    public GameObject galleryPanel;
    public RawImage galleryImage;
    public TextMeshProUGUI photoCounterText;
    public TextMeshProUGUI galleryCounterText;

    [Header("Egyéb")]
    public GameObject crosshair;
    public MonoBehaviour fpsController;
    public GameManager gameManager;
    public MainLevelManager mainLevelManager;

    [Header("Szellem kapcsolat")]
    public EntityVisibility ghostVisibility;

    [Header("Beállítások")]
    public int maxPhotos = 5;
    public float flashDuration = 0.08f;

    private bool isCameraEquipped = false;
    private bool isGalleryOpen = false;
    private bool isTakingPhoto = false;

    private List<Texture2D> savedPhotos = new List<Texture2D>();
    private int currentPhotoIndex = 0;

    private void Start()
    {
        // Kamera alapból kikapcsolva
        if (photoCamera != null)
            photoCamera.enabled = false;

        // Vaku kikapcsolva
        if (flashLight != null)
            flashLight.enabled = false;

        // UI elemek kikapcsolása
        if (cameraOverlay != null)
            cameraOverlay.SetActive(false);

        if (galleryPanel != null)
            galleryPanel.SetActive(false);

        UpdatePhotoCounter();
    }

    private void Update()
    {
        // Game Over alatt semmi input ne működjön
        if (mainLevelManager != null && mainLevelManager.IsGameOver())
            return;

        // Email közben se lehessen fotózni
        if (gameManager != null && gameManager.IsEmailOpen())
            return;

        HandleCameraToggle();
        HandleGalleryToggle();

        // Ha galéria nyitva van, csak azt kezeljük
        if (isGalleryOpen)
        {
            HandleGalleryNavigation();
            return;
        }

        // Fotózás csak ha kamera aktív
        if (isCameraEquipped)
            HandlePhotoInput();
    }

    // Kamera ki/be kapcsolása (F)
    private void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isGalleryOpen)
                return;

            isCameraEquipped = !isCameraEquipped;

            if (photoCamera != null)
                photoCamera.enabled = isCameraEquipped;

            if (cameraOverlay != null)
                cameraOverlay.SetActive(isCameraEquipped);

            if (crosshair != null)
                crosshair.SetActive(!isCameraEquipped);

            // Szellemnek jelezzük
            if (ghostVisibility != null)
                ghostVisibility.SetPlayerLookingThroughCamera(isCameraEquipped);
        }
    }

    // Fotó készítés (jobb klikk)
    private void HandlePhotoInput()
    {
        if (Input.GetMouseButtonDown(1) && !isTakingPhoto)
            StartCoroutine(TakePhoto());
    }

    // Galéria nyitás/zárás (TAB)
    private void HandleGalleryToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (savedPhotos.Count == 0)
                return;

            isGalleryOpen = !isGalleryOpen;

            if (galleryPanel != null)
                galleryPanel.SetActive(isGalleryOpen);

            if (isGalleryOpen)
            {
                ShowPhoto(currentPhotoIndex);

                if (fpsController != null)
                    fpsController.enabled = false;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (fpsController != null)
                    fpsController.enabled = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Galéria navigáció
    private void HandleGalleryNavigation()
    {
        if (savedPhotos.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            currentPhotoIndex = (currentPhotoIndex + 1) % savedPhotos.Count;
            ShowPhoto(currentPhotoIndex);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            currentPhotoIndex--;
            if (currentPhotoIndex < 0)
                currentPhotoIndex = savedPhotos.Count - 1;

            ShowPhoto(currentPhotoIndex);
        }
    }

    // Fotó készítés coroutine
    private IEnumerator TakePhoto()
    {
        if (savedPhotos.Count >= maxPhotos)
            yield break;

        isTakingPhoto = true;

        // vaku felvillan
        if (flashLight != null)
            flashLight.enabled = true;

        yield return new WaitForSeconds(flashDuration);
        yield return new WaitForEndOfFrame();

        Texture2D newPhoto = CapturePhoto();

        if (flashLight != null)
            flashLight.enabled = false;

        if (newPhoto != null)
        {
            savedPhotos.Add(newPhoto);

            // Megnézzük rajta van-e a szellem
            bool containsEntity = ghostVisibility != null && ghostVisibility.IsVisibleForEvidence();

            // Globális archívumba mentjük
            MissionPhotoArchive.AddPhoto(newPhoto, containsEntity);

            currentPhotoIndex = savedPhotos.Count - 1;
            UpdatePhotoCounter();

            if (ghostVisibility != null)
                ghostVisibility.OnPhotoTaken();
        }

        isTakingPhoto = false;
    }

    // RenderTexture → Texture2D
    private Texture2D CapturePhoto()
    {
        RenderTexture.active = photoRenderTexture;
        photoCamera.targetTexture = photoRenderTexture;
        photoCamera.Render();

        Texture2D photo = new Texture2D(photoRenderTexture.width, photoRenderTexture.height);
        photo.ReadPixels(new Rect(0, 0, photo.width, photo.height), 0, 0);
        photo.Apply();

        photoCamera.targetTexture = null;
        RenderTexture.active = null;

        return photo;
    }

    private void ShowPhoto(int index)
    {
        if (galleryImage != null)
            galleryImage.texture = savedPhotos[index];

        if (galleryCounterText != null)
            galleryCounterText.text = (index + 1) + " / " + savedPhotos.Count;
    }

    private void UpdatePhotoCounter()
    {
        if (photoCounterText != null)
            photoCounterText.text = savedPhotos.Count + " / " + maxPhotos;
    }

    public int GetPhotoCount()
    {
        return savedPhotos.Count;
    }

    // Game Over esetén minden kamera UI bezárása
    public void ForceCloseCameraUI()
    {
        isCameraEquipped = false;
        isGalleryOpen = false;

        if (photoCamera != null)
            photoCamera.enabled = false;

        if (cameraOverlay != null)
            cameraOverlay.SetActive(false);

        if (galleryPanel != null)
            galleryPanel.SetActive(false);
    }
}