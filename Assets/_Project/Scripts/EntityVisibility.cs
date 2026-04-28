using UnityEngine;

[RequireComponent(typeof(EntityBrain))]
public class EntityVisibility : MonoBehaviour
{
    [Header("Láthatóság")]
    public bool canBeSeenNormally = false;
    public int photosNeededToManifest = 1;
    public float threatIncreaseOnPhoto = 15f;

    [Header("Jumpscare esély")]
    public float earlyJumpscareChance = 0.10f;
    public float midJumpscareChance = 0.40f;
    public float lateJumpscareChance = 0.85f;

    public bool isPlayerLookingThroughCamera { get; private set; } = false;

    private int photoCount = 0;
    private Renderer[] visualRenderers;
    private EntityBrain brain;

    private void Awake()
    {
        brain = GetComponent<EntityBrain>();

        // Sketchfab modelleknél gyakran több renderer van,
        // ezért automatikusan összeszedjük az összes gyerek renderert.
        visualRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start()
    {
        UpdateVisibility();
    }

    public void SetPlayerLookingThroughCamera(bool isLooking)
    {
        // A PhotoCameraSystem jelzi, hogy a játékos épp kamera módban van-e.
        isPlayerLookingThroughCamera = isLooking;
        UpdateVisibility();
    }

    public void UpdateVisibility()
    {
        // Chase és jumpscare alatt mindig látszódjon.
        if (brain.currentState == EntityBrain.EntityState.Chase ||
            brain.currentState == EntityBrain.EntityState.Jumpscare ||
            canBeSeenNormally)
        {
            SetRenderers(true);
            return;
        }

        // Alapból csak akkor látszik, ha a játékos kamerán át néz.
        SetRenderers(isPlayerLookingThroughCamera);
    }

    public void SetRenderers(bool visible)
    {
        if (visualRenderers == null)
            return;

        foreach (Renderer rend in visualRenderers)
        {
            if (rend != null)
                rend.enabled = visible;
        }
    }

    public void OnPhotoTaken()
    {
        // Csak kamera módban készült fotó számít a szellemre.
        if (!isPlayerLookingThroughCamera)
            return;

        photoCount++;

        // Fotózás növeli a fenyegetettséget.
        brain.threatLevel = Mathf.Clamp(
            brain.threatLevel + threatIncreaseOnPhoto,
            0f,
            brain.maxThreat
        );

        // Adott számú fotó után a szellem normál nézetben is látható.
        if (photoCount >= photosNeededToManifest)
        {
            canBeSeenNormally = true;
            UpdateVisibility();
        }

        TryPhotoJumpscare();
    }

    private void TryPhotoJumpscare()
    {
        if (brain.currentState == EntityBrain.EntityState.Jumpscare)
            return;

        float chance = earlyJumpscareChance;

        if (brain.threatLevel > 35f)
            chance = midJumpscareChance;

        if (brain.threatLevel > 75f)
            chance = lateJumpscareChance;

        // Véletlen esély alapján jumpscare történhet.
        if (Random.value <= chance)
        {
            brain.TriggerJumpscare();
        }
        // Ha nincs jumpscare, akkor normál reakció indulhat.
        else if (brain.currentState == EntityBrain.EntityState.Watching ||
                 brain.currentState == EntityBrain.EntityState.Observed)
        {
            brain.TriggerObserved();
        }
    }
    public bool IsVisibleForEvidence()
    {
        if (visualRenderers == null)
            return false;

        foreach (Renderer rend in visualRenderers)
        {
            if (rend != null && rend.enabled)
                return true;
        }

        return false;
    }
}