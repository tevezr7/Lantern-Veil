using UnityEngine;

public class EnemyDeathSFX : MonoBehaviour
{
    [Header("Death Sound")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField][Range(0f, 1f)] private float volume = 1f;

    
    public void PlayDeathSound()
    {
        if (deathClip == null) return;

        
        AudioSource.PlayClipAtPoint(deathClip, transform.position, volume);
    }
}
