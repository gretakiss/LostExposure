using UnityEngine;

public class DoorHinge : MonoBehaviour
{
    [Header("Door Angles (degrees)")]
    [SerializeField] private float closedYaw = 0f;
    [SerializeField] private float openYaw = 85f;

    [Header("Animation")]
    [SerializeField] private float openCloseTime = 0.6f;
    [SerializeField] private AnimationCurve easing = null;

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    private bool isOpen;
    private bool isAnimating;
    private float baseYaw;

    private void Awake()
    {
        // A jelenlegi y rotation lesz a zárt állapot
        baseYaw = transform.localEulerAngles.y;

        if (easing == null)
        {
            easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        isOpen = startOpen;
        ApplyImmediate();
    }

    public void Toggle()
    {
        if (isAnimating) return;
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(AnimateDoor(isOpen));
    }

    public bool IsAnimating => isAnimating;

    private void ApplyImmediate()
    {
        float target = GetTargetYaw(isOpen);
        var e = transform.localEulerAngles;
        e.y = target;
        transform.localEulerAngles = e;
    }

    private float GetTargetYaw(bool open)
    {
        // A zárt a baseYaw + closedYaw, nyitott a baseYaw + openYaw
        return baseYaw + (open ? openYaw : closedYaw);
    }

    private System.Collections.IEnumerator AnimateDoor(bool open)
    {
        isAnimating = true;

        float start = transform.localEulerAngles.y;
        float end = GetTargetYaw(open);

        start = NormalizeAngle(start);
        end = NormalizeAngle(end);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openCloseTime);
            float eased = easing.Evaluate(Mathf.Clamp01(t));
            float y = Mathf.LerpAngle(start, end, eased);

            var e = transform.localEulerAngles;
            e.y = y;
            transform.localEulerAngles = e;

            yield return null;
        }

        ApplyImmediate();
        isAnimating = false;
    }

    private static float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }
}
