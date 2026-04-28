using System.Collections;
using UnityEngine;

public class EntityBrain : MonoBehaviour
{
    public enum EntityState
    {
        Dormant,
        Watching,
        Observed,
        React,
        Jumpscare,
        Chase
    }

    [Header("Állapot és veszélyszint")]
    public EntityState currentState = EntityState.Dormant;
    public float threatLevel = 0f;
    public float maxThreat = 100f;
    public float threatIncreaseOverTime = 0.5f;

    [Header("Referenciák")]
    public Transform playerCamera;

    [HideInInspector] public EntityVisibility visibility;
    [HideInInspector] public EntityMovement movement;
    [HideInInspector] public EntityAudio audioManager;

    [Header("Reakció idõk")]
    public float reactDelayManifested = 0.15f;
    public float reactDelayUnmanifested = 3.5f;
    public float hiddenTime = 1f;

    [Header("Jumpscare")]
    public float jumpscareDuration = 1.5f;

    public Coroutine activeRoutine;

    private void Awake()
    {
        // A szellem mûködése több kisebb scriptre van bontva.
        visibility = GetComponent<EntityVisibility>();
        movement = GetComponent<EntityMovement>();
        audioManager = GetComponent<EntityAudio>();
    }

    private void Start()
    {
        // Ha nincs kézzel bekötve, automatikusan megkeressük a fõ kamerát.
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        // A fenyegetettségi szint idõvel fokozatosan nõ.
        // Jumpscare és chase alatt ezt nem növeljük tovább.
        if (currentState != EntityState.Jumpscare && currentState != EntityState.Chase)
        {
            threatLevel = Mathf.Clamp(
                threatLevel + threatIncreaseOverTime * Time.deltaTime,
                0f,
                maxThreat
            );
        }
    }

    public void SetState(EntityState newState)
    {
        // Központi állapotváltás.
        if (currentState == newState)
            return;

        currentState = newState;
    }

    public void TriggerObserved()
    {
        // Ha a játékos megfigyeli a szellemet, reakció indul.
        if (currentState == EntityState.Watching && activeRoutine == null)
            activeRoutine = StartCoroutine(DoReact());
    }

    public void TriggerChase()
    {
        // Üldözés indítása.
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        SetState(EntityState.Chase);

        if (visibility != null)
            visibility.UpdateVisibility();
    }

    public void TriggerJumpscare()
    {
        // Fotózás utáni jumpscare indítása.
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(JumpscareRoutine());
    }

    private IEnumerator DoReact()
    {
        SetState(EntityState.React);

        // Ha már manifestálódott, gyorsabban reagál.
        // Ha még csak kamerán át látható, több idõt hagyunk a fotó elkészítésére.
        float delay = visibility != null && visibility.canBeSeenNormally
            ? reactDelayManifested
            : reactDelayUnmanifested;

        yield return new WaitForSeconds(delay);

        if (visibility != null)
            visibility.SetRenderers(false);

        yield return new WaitForSeconds(hiddenTime);

        if (movement != null)
            movement.TeleportToRandomPoint();

        SetState(EntityState.Dormant);

        if (visibility != null)
            visibility.UpdateVisibility();

        activeRoutine = null;
    }

    private IEnumerator JumpscareRoutine()
    {
        SetState(EntityState.Jumpscare);

        if (visibility != null)
            visibility.UpdateVisibility();

        if (movement != null)
            movement.SnapToJumpscarePosition(playerCamera);

        if (audioManager != null)
            audioManager.PlayJumpscare();

        yield return new WaitForSeconds(jumpscareDuration);

        if (visibility != null)
            visibility.SetRenderers(false);

        if (movement != null)
            movement.TeleportToRandomPoint();

        SetState(EntityState.Dormant);

        if (visibility != null)
            visibility.UpdateVisibility();

        activeRoutine = null;
    }
}