using System;
using UnityEngine;

public class PotionInventory : MonoBehaviour
{
    public int potion_counter;

    [SerializeField] private AudioClip drinkPotionSfx;     
    [SerializeField] private AudioSource sfxSource;

    public Action<int> OnPotionChanged;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPotion()
    {
        potion_counter++;
        OnPotionChanged?.Invoke(potion_counter); 


        var ui = GetComponentInChildren<PlayerUI>();
        if (ui != null)
            ui.UpdatePotionCount();
    }

    public void UsePotion()
    {
        if (potion_counter > 0)
        {
            potion_counter--;
            OnPotionChanged?.Invoke(potion_counter); 
            PlayDrinkPotionSound();
            var ui = GetComponentInChildren<PlayerUI>();
            if (ui != null)
                ui.UpdatePotionCount();
        }
    }

    private void PlayDrinkPotionSound()
    {
        if (drinkPotionSfx == null) return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(drinkPotionSfx);
        }
        else
        {
            AudioSource.PlayClipAtPoint(drinkPotionSfx, transform.position);
        }
    }
}
