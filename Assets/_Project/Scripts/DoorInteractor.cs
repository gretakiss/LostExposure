using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float interactDistance = 2.2f;
    [SerializeField] private LayerMask doorMask;

    [Header("Input")]
    [SerializeField] private Key interactKey = Key.E;

    private void Reset()
    {
        doorMask = ~0;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current[interactKey].wasPressedThisFrame) return;

        var cam = GetComponent<Camera>();
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, doorMask, QueryTriggerInteraction.Ignore))
        {
            DoorHinge hinge = hit.collider.GetComponentInParent<DoorHinge>();
            if (hinge != null && !hinge.IsAnimating)
            {
                hinge.Toggle();
            }
        }
    }
}
