using UnityEngine;

public class Potion : Interactable
{
    public enum PotionType
    {
        Health,
        Magic
    }

    [Header("Potion Type")]
    [SerializeField] private PotionType potionType = PotionType.Health;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private AudioSource sfxSource;

    protected override void Interact()
    {
        PlayPickupSound();

        switch (potionType)
        {
            case PotionType.Health:
                {
                    var healthInv = player.GetComponent<PotionInventory>();
                    if (healthInv != null)
                    {
                        healthInv.AddPotion();
                    }
                    break;
                }

            case PotionType.Magic:
                {
                    var magicInv = player.GetComponent<MagicPotionInventory>();
                    if (magicInv != null)
                    {
                        magicInv.AddMagicPotion();
                    }
                    break;
                }
        }

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
