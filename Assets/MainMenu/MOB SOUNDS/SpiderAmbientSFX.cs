using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpiderAmbientSFX : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] chitterClips;

    [Header("Timing")]
    [SerializeField] private Vector2 delayRange = new Vector2(2f, 5f); // spiders = more frequent

    [Header("Distance")]
    [SerializeField] private float maxDistanceToPlayer = 20f;

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
        if (chitterClips == null || chitterClips.Length == 0)
            return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (player != null && Vector3.Distance(transform.position, player.position) > maxDistanceToPlayer)
        {
            ResetTimer();
            return;
        }

        // Play random spider sound
        AudioClip clip = chitterClips[Random.Range(0, chitterClips.Length)];
        src.PlayOneShot(clip);

        ResetTimer();
    }

    void ResetTimer()
    {
        timer = Random.Range(delayRange.x, delayRange.y);
    }
}
