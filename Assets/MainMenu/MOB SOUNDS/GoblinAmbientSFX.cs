using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GoblinAmbientSFX : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] idleClips;

    [Header("Timing")]
    [SerializeField] private float minDelay = 3f;   // minimum seconds between sounds
    [SerializeField] private float maxDelay = 7f;   // maximum seconds between sounds

    [Header("Distance")]
    [SerializeField] private float maxDistanceToPlayer = 25f; // only play if player is near

    AudioSource src;
    Transform player;
    float timer;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;                 // 3D sound
        src.rolloffMode = AudioRolloffMode.Linear;
        src.maxDistance = maxDistanceToPlayer;
    }

    void Start()
    {
        // find player once
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // first sound after a random delay
        timer = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        if (idleClips == null || idleClips.Length == 0)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        // Only make noise if player is nearby
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > maxDistanceToPlayer)
            {
                ResetTimer();
                return;
            }
        }

        // Pick a random goblin sound and play it
        AudioClip clip = idleClips[Random.Range(0, idleClips.Length)];
        if (clip != null)
            src.PlayOneShot(clip);

        ResetTimer();
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }
}
