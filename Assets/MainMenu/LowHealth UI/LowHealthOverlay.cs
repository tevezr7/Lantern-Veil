using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class LowHealthFX : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Image overlay;
    [SerializeField] private AudioSource heartbeat;
    [SerializeField] private PlayerHealth player;

    [Header("Thresholds")]
    [Range(0f, 1f)] public float triggerFraction = 0.5f;

    [Header("Visuals")]
    [Tooltip("Max overlay opacity at 0 HP")]
    [Range(0f, 1f)] public float maxAlpha = 0.35f;
    [Tooltip("Pulse speed at 0 HP (beats per second)")]
    public float maxPulseHz = 2.2f;
    [Tooltip("Pulse speed just under threshold")]
    public float minPulseHz = 0.8f;

    [Header("Audio")]
    [Range(0f, 1f)] public float maxHeartbeatVolume = 0.55f;
    [Tooltip("Pitch at 0 HP")]
    public float maxHeartbeatPitch = 1.25f;
    [Tooltip("Pitch just under threshold")]
    public float minHeartbeatPitch = 0.9f;

    [Header("Audio Mixer Lowpass (optional)")]
    public bool useMixerLowpass = true;
    [SerializeField] private AudioMixer masterMixer;          // drag your Master mixer
    [SerializeField] private string cutoffParam = "LowpassHz"; // exposed param name
    [Tooltip("Cutoff at 0 HP (strong muffle)")] public float minCutoff = 800f;
    [Tooltip("Cutoff when healthy (no muffle)")] public float maxCutoff = 22000f;

    float currentCutoff;   // smoothing
    float targetAlpha;
    float currentAlpha;

    void Reset()
    {
        overlay = GetComponent<Image>();
        heartbeat = GetComponent<AudioSource>();
    }

    void Awake()
    {
        if (!overlay) overlay = GetComponent<Image>();
        if (!player)
        {
#if UNITY_2023_1_OR_NEWER
            player = Object.FindAnyObjectByType<PlayerHealth>();
#else
            player = FindObjectOfType<PlayerHealth>();
#endif
        }

        if (overlay)
        {
            var c = overlay.color; c.a = 0f; overlay.color = c;
        }

        if (heartbeat)
        {
            heartbeat.playOnAwake = false;
            heartbeat.loop = true;
            heartbeat.volume = 0f;
        }

        currentCutoff = maxCutoff;
        if (useMixerLowpass && masterMixer) masterMixer.SetFloat(cutoffParam, maxCutoff);
    }

    void Update()
    {
        if (!player || !overlay) return;

        float hpFrac = Mathf.Clamp01(player.health / Mathf.Max(1f, player.maxHealth));

        // t = 0 just under threshold, 1 at 0 HP (always defined)
        float t = (hpFrac < triggerFraction)
            ? Mathf.InverseLerp(triggerFraction, 0f, hpFrac)
            : 0f;

        if (hpFrac < triggerFraction)
        {
            // Pulse speed based on how low HP is
            float pulseHz = Mathf.Lerp(minPulseHz, maxPulseHz, t);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (pulseHz * Mathf.PI * 2f));

            // Base alpha + pulse
            float baseAlpha = Mathf.Lerp(0.05f, maxAlpha, t);
            targetAlpha = Mathf.Clamp01(baseAlpha * (0.7f + 0.3f * pulse));

            // Heartbeat
            if (heartbeat && !heartbeat.isPlaying) heartbeat.Play();
            if (heartbeat)
            {
                heartbeat.volume = Mathf.Lerp(0f, maxHeartbeatVolume, t);
                heartbeat.pitch = Mathf.Lerp(minHeartbeatPitch, maxHeartbeatPitch, t);
            }
        }
        else
        {
            targetAlpha = 0f;

            if (heartbeat && heartbeat.isPlaying)
            {
                heartbeat.volume = Mathf.MoveTowards(heartbeat.volume, 0f, Time.unscaledDeltaTime * 2f);
                if (heartbeat.volume <= 0.001f) heartbeat.Stop();
            }
        }

        // Overlay fade
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.unscaledDeltaTime * 2.5f);
        var col = overlay.color; col.a = currentAlpha; overlay.color = col;

        // Low-pass muffle (optional via mixer)
        if (useMixerLowpass && masterMixer)
        {
            float targetCutoff = (hpFrac < triggerFraction)
                ? Mathf.Lerp(maxCutoff, minCutoff, t)   // more muffle as HP drops
                : maxCutoff;                             // clear when healthy

            currentCutoff = Mathf.MoveTowards(currentCutoff, targetCutoff, Time.unscaledDeltaTime * 4000f);
            masterMixer.SetFloat(cutoffParam, currentCutoff);
        }
    }
    public void ForceClear()
    {
       
        targetAlpha = 0f;
        currentAlpha = 0f;
        if (overlay)
        {
            var c = overlay.color; c.a = 0f; overlay.color = c;
        }

       
        if (heartbeat)
        {
            heartbeat.volume = 0f;
            heartbeat.Stop();
        }

       
        if (useMixerLowpass && masterMixer)
            masterMixer.SetFloat(cutoffParam, maxCutoff);
    }

    void OnDisable()
    {
        
        if (heartbeat) heartbeat.Stop();
        if (useMixerLowpass && masterMixer)
            masterMixer.SetFloat(cutoffParam, maxCutoff);
    }

}
