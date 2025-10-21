using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Potion : Interactable
{

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSfx;         
    [SerializeField] private AudioSource sfxSource;

    protected override void Interact()
    {
        var inventory = player.GetComponent<PotionInventory>();
        if (inventory == null) return;

        PlayPickupSound();

        inventory.AddPotion();
        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (pickupSfx == null) return;

        
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(pickupSfx);
        }
        else
        {
            
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
        }
    }
}
