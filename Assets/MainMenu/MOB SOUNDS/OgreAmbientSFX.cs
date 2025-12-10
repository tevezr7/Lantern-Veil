using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OgreAmbientSFX : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] ambientClips;

    [Header("Timing")]
    [SerializeField] private Vector2 delayRange = new Vector2(6f, 12f); // slow & heavy

    [Header("Distance")]
    [SerializeField] private float maxDistanceToPlayer = 30f;

    private AudioSource src;
    private Transform player;
    private float timer;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.maxDistance = maxDistanceToPlayer;
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        ResetTimer();
    }

    void Update()
    {
        if (ambientClips == null || ambientClips.Length == 0)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (player != null && Vector3.Distance(transform.position, player.position) > maxDistanceToPlayer)
        {
            ResetTimer();
            return;
        }

        // Play deep ogre sound
        AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
        src.PlayOneShot(clip);

        ResetTimer();
    }

    void ResetTimer()
    {
        timer = Random.Range(delayRange.x, delayRange.y);
    }
}
