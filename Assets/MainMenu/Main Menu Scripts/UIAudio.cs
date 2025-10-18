using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIAudio : MonoBehaviour
{
    public static UIAudio I;
    public AudioClip hover;
    public AudioClip click;

    AudioSource src;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D
    }

    public void PlayHover() { if (hover) src.PlayOneShot(hover); }
    public void PlayClick() { if (click) src.PlayOneShot(click); }
}
