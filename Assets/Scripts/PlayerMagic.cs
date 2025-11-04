using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using JetBrains.Annotations;

public class PlayerMagic : MonoBehaviour
{
    private PlayerCombat playerCombat;
    [Header("Magic Bar")]
    public float magic;
    private float lerpTimer;
    public float maxMagic = 100f;
    public float chipSpeed = 2f;
    public Image frontMagicBar;
    public Image backMagicBar;
    private float lastSpellCost;   // internal record
    public float LastSpellCost => lastSpellCost; // allows read-only access
    void Start()
    {
        magic = maxMagic;
        playerCombat = GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        magic = Mathf.Clamp(magic, 0, maxMagic);
        UpdateMagicUI();
    }

    void LateUpdate()
    {
        // auto-reset EACH FRAME, after everyone else has run Update()
        lastSpellCost = 0f;
    }

    public void UpdateMagicUI()
    {
        Debug.Log("Magic: " + magic);
        float fillF = frontMagicBar.fillAmount;
        float fillB = backMagicBar.fillAmount;
        float mFraction = magic / maxMagic; //magic fraction, value between 0 and 1
        if (fillB > mFraction)
        {
            frontMagicBar.fillAmount = mFraction; //immediate
            backMagicBar.color = Color.softYellow; //yellow
            lerpTimer += Time.deltaTime; //time between current and previous frame
            float percentComplete = lerpTimer / chipSpeed; // from 0 to 1
            percentComplete *= percentComplete; //square percentComplete to slow down the animation
            backMagicBar.fillAmount = Mathf.Lerp(fillB, mFraction, percentComplete); // Smoothly animate the back (damage) health bar from its current fill (fillB) toward the target health (hFraction) using percentComplete as the 0–1 lerp factor.
        }
        if (fillF < mFraction)
        {
            backMagicBar.fillAmount = mFraction; //immediate
            backMagicBar.color = Color.green; //green
            lerpTimer += Time.deltaTime; //time between current and previous frame
            float percentComplete = lerpTimer / chipSpeed; // from 0 to 1
            percentComplete *= percentComplete; //square percentComplete to slow the animation speed
            frontMagicBar.fillAmount = Mathf.Lerp(fillF, mFraction, percentComplete); // Smoothly animate the back (damage) health bar from its current fill (fillB) toward the target health (hFraction) using percentComplete as the 0–1 lerp factor.
        }
    }

    public void UseMagic(float amount)
    {
            magic -= amount;
            lastSpellCost = amount;
            lerpTimer = 0f; //reset timer for magic bar animation
    }
}
