using UnityEngine;

[RequireComponent(typeof(EntityBrain))]
public class EntityMovement : MonoBehaviour
{
    [Header("Üldözés")]
    public float chaseSpeed = 3.5f;
    public float catchDistance = 1.2f;
    public float giveUpDistance = 15f;

    [Header("Teleportálás")]
    public Transform[] teleportPoints;

    [Header("Jumpscare")]
    public float jumpscareDistance = 0.8f;

    [Header("Referenciák")]
    public MainLevelManager levelManager;

    private EntityBrain brain;

    private void Awake()
    {
        brain = GetComponent<EntityBrain>();
    }

    private void Update()
    {
        if (brain.currentState == EntityBrain.EntityState.Chase)
            DoChase();
    }

    private void DoChase()
    {
        if (brain.playerCamera == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, brain.playerCamera.position);

        if (distanceToPlayer >= giveUpDistance)
        {
            TeleportToRandomPoint();
            brain.SetState(EntityBrain.EntityState.Dormant);

            if (brain.visibility != null)
                brain.visibility.UpdateVisibility();

            return;
        }

        Vector3 targetPos = new Vector3(
            brain.playerCamera.position.x,
            transform.position.y,
            brain.playerCamera.position.z
        );

        transform.LookAt(targetPos);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            chaseSpeed * Time.deltaTime
        );

        float newDistance = Vector3.Distance(transform.position, targetPos);

        if (newDistance <= catchDistance)
        {
            Debug.Log("A szellem elkapta a játékost.");

            if (levelManager != null)
                levelManager.TriggerGameOver();

            brain.SetState(EntityBrain.EntityState.Dormant);
        }
    }

    public void TeleportToRandomPoint()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
            return;

        Transform target = teleportPoints[Random.Range(0, teleportPoints.Length)];

        if (target == null)
            return;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void SnapToJumpscarePosition(Transform cameraTransform)
    {
        if (cameraTransform == null)
            return;

        Vector3 forwardPos = cameraTransform.position + cameraTransform.forward * jumpscareDistance;

        transform.position = new Vector3(
            forwardPos.x,
            transform.position.y,
            forwardPos.z
        );

        Vector3 lookAtPos = cameraTransform.position;
        lookAtPos.y = transform.position.y;

        transform.LookAt(lookAtPos);
    }
}