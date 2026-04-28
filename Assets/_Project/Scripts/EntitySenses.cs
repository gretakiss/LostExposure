using UnityEngine;

[RequireComponent(typeof(EntityBrain))]
public class EntitySenses : MonoBehaviour
{
    [Header("Érzékelés")]
    public float awareDistance = 8f;
    public float observeDistance = 6f;
    public float observeAngle = 15f;
    public float scareDistance = 2.5f;
    public float observeTimeNeeded = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Üldözés feltételei")]
    public float chaseThreatThreshold = 75f;
    public float tooCloseCooldown = 2f;

    private EntityBrain brain;
    private float observeTimer = 0f;
    private float lastTooCloseTime = -999f;

    private void Awake()
    {
        brain = GetComponent<EntityBrain>();
    }

    private void Update()
    {
        if (brain.playerCamera == null)
            return;

        // Reakció és jumpscare alatt nem vizsgálunk új érzékelést.
        if (brain.currentState == EntityBrain.EntityState.Jumpscare ||
            brain.currentState == EntityBrain.EntityState.React)
            return;

        CheckDistances();
    }

    private void CheckDistances()
    {
        float dist = Vector3.Distance(transform.position, brain.playerCamera.position);

        // Ha a játékos túl közel kerül, külön reakció történik.
        if (dist <= scareDistance && brain.currentState != EntityBrain.EntityState.Chase)
        {
            CheckTooClose();
            return;
        }

        // Ha a játékos elég közel van, a szellem figyelõ állapotba kerül.
        if (brain.currentState == EntityBrain.EntityState.Dormant && dist <= awareDistance)
        {
            brain.SetState(EntityBrain.EntityState.Watching);
        }

        // Figyelõ állapotban vizsgáljuk, hogy a játékos ténylegesen nézi-e.
        if (brain.currentState == EntityBrain.EntityState.Watching)
        {
            CheckObserved(dist);
        }
    }

    private void CheckTooClose()
    {
        // Cooldown, hogy ne fusson le túl gyakran ugyanaz a reakció.
        if (Time.time < lastTooCloseTime + tooCloseCooldown)
            return;

        lastTooCloseTime = Time.time;

        bool canChase =
            brain.visibility != null &&
            brain.visibility.canBeSeenNormally &&
            brain.threatLevel >= chaseThreatThreshold;

        if (canChase)
        {
            brain.TriggerChase();
        }
        else
        {
            brain.TriggerObserved();
        }
    }

    private void CheckObserved(float dist)
    {
        if (dist > observeDistance)
        {
            observeTimer = 0f;
            return;
        }

        Vector3 dir = (transform.position - brain.playerCamera.position).normalized;
        float angle = Vector3.Angle(brain.playerCamera.forward, dir);

        bool isVisibleToPlayer =
            brain.visibility != null &&
            (brain.visibility.canBeSeenNormally || brain.visibility.isPlayerLookingThroughCamera);

        // Csak akkor számít megfigyeltnek, ha a játékos tényleg ránéz és nincs fal köztük.
        if (angle <= observeAngle && HasLineOfSight() && isVisibleToPlayer)
        {
            observeTimer += Time.deltaTime;

            if (observeTimer >= observeTimeNeeded)
            {
                brain.TriggerObserved();
                observeTimer = 0f;
            }
        }
        else
        {
            observeTimer = 0f;
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 dir = transform.position - brain.playerCamera.position;

        return !Physics.Raycast(
            brain.playerCamera.position,
            dir.normalized,
            dir.magnitude,
            obstacleLayer
        );
    }
}