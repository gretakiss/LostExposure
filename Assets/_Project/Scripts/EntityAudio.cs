using UnityEngine;

[RequireComponent(typeof(EntityBrain))]
public class EntityAudio : MonoBehaviour
{
    [Header("Hangforrások")]
    public AudioSource watchingAudio;
    public AudioSource jumpscareAudio;

    [Header("Beállítások")]
    public float watchingAudioDistance = 8f;

    private EntityBrain brain;

    private void Awake()
    {
        brain = GetComponent<EntityBrain>();
    }

    private void Update()
    {
        if (brain.playerCamera == null)
            return;

        if (watchingAudio == null)
            return;

        // A figyelõ hang akkor szól, ha a szellem figyel vagy üldöz, és elég közel van.
        bool shouldPlay =
            (brain.currentState == EntityBrain.EntityState.Watching ||
             brain.currentState == EntityBrain.EntityState.Chase) &&
            Vector3.Distance(transform.position, brain.playerCamera.position) <= watchingAudioDistance;

        if (shouldPlay && !watchingAudio.isPlaying)
            watchingAudio.Play();
        else if (!shouldPlay && watchingAudio.isPlaying)
            watchingAudio.Stop();
    }

    public void PlayJumpscare()
    {
        if (jumpscareAudio != null)
            jumpscareAudio.Play();
    }
}