using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{ 
    private PlayerCombat playerCombat;

    [Header("Health Bar")]
    public float health;
    private float lerpTimer;
    public float maxHealth = 100f;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;
    private float lastDamageTaken;   // internal record
    public float LastDamageTaken => lastDamageTaken; // allows read-only access

    [Header("Audio")]
    public AudioSource playerAudio;
    public AudioClip damageClip;

    [Header("Events")]
    public UnityEvent onDied;      
    private bool isDead = false;     

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        playerCombat = GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();

     
        if (!isDead && health <= 0f)
        {
            isDead = true;
            onDied?.Invoke();
        }
    }

    void LateUpdate()
    {
        // auto-reset EACH FRAME, after everyone else has run Update()
        lastDamageTaken = 0f;
    }

    public void UpdateHealthUI()
    {
        Debug.Log("Health: " + health);
        float fillF = frontHealthBar.fillAmount;
        float fillB = backHealthBar.fillAmount;
        float hFraction = health / maxHealth; //health fraction, value between 0 and 1
        if (fillB > hFraction)
        {
            frontHealthBar.fillAmount = hFraction; //immediate
            backHealthBar.color = Color.softYellow; //yellow
            lerpTimer += Time.deltaTime; //time between current and previous frame  
            float percentComplete = lerpTimer / chipSpeed; // from 0 to 1
            percentComplete *= percentComplete; //square percentComplete to slow down the animation
            backHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete); // Smoothly animate the back (damage) health bar from its current fill (fillB) toward the target health (hFraction) using percentComplete as the 0–1 lerp factor.
        }
        if (fillF < hFraction)
        {
            backHealthBar.fillAmount = hFraction; //immediate
            backHealthBar.color = Color.green; //green
            lerpTimer += Time.deltaTime; //time between current and previous frame
            float percentComplete = lerpTimer / chipSpeed; // from 0 to 1
            percentComplete *= percentComplete; //square percentComplete to slow the animation speed
            frontHealthBar.fillAmount = Mathf.Lerp(fillF, hFraction, percentComplete); // Smoothly animate the back (damage) health bar from its current fill (fillB) toward the target health (hFraction) using percentComplete as the 0–1 lerp factor.
        }
    }

    public void TakeDamage (float damage, Vector3 attackerPosition)
    {
        lastDamageTaken = damage;
        if (playerCombat != null)
        {
            damage = playerCombat.PreventDamage(damage, attackerPosition);
        }
        if (damage <= 0f) return; // no damage taken   
        health -= damage; 
        lerpTimer = 0f;
        if (playerAudio && damageClip) playerAudio.PlayOneShot(damageClip);
    }

    public void TakeDamage(float damage) // overload for when attacker position is unknown, unity Events handling
    {
        TakeDamage(damage, transform.position); // fallback attacker pos = self
    }

    public void RestoreHealth (float healAmount)
    {
        health += healAmount;
        lerpTimer = 0f;
        if (isDead && health > 0f) isDead = false;
    }
    public void DrinkPotion()
    {
        if (health >= maxHealth)
            return;
        var inventory = GetComponent<PotionInventory>();
        if (inventory != null && inventory.potion_counter > 0)
        {
            inventory.UsePotion();
            RestoreHealth(50);

        }
    }


}

