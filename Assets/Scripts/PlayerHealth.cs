using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{ 
    [Header("Health Bar")]
    private float health;
    private float lerpTimer;
    public float maxHealth = 100f;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;

    [Header("Audio")]
    public AudioSource playerAudio;
    public AudioClip damageClip;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
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

    public void TakeDamage (float damage)
    {
        health -= damage; 
        lerpTimer = 0f; 
        playerAudio.PlayOneShot(damageClip);
    }

    public void RestoreHealth (float healAmount)
    {
        health += healAmount;
        lerpTimer = 0f;
    }


}
