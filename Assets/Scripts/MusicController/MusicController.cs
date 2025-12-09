using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    public AudioSource source;

    public AudioClip ambientMusic;
    public AudioClip combatMusic;

    [SerializeField] private float ambientVolume = 0.25f;
    [SerializeField] private float combatVolume = 0.35f;

    void Start()
    {
        PlayAmbient();
    }

    public void PlayAmbient()
    {
        source.volume = ambientVolume;
        source.clip = ambientMusic;
        source.loop = true;
        source.Play();
    }

    public void PlayCombat()
    {
        source.volume = combatVolume;
        source.clip = combatMusic;
        source.loop = true;
        source.Play();
    }

}
