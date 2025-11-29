using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIAudio : MonoBehaviour
{
    public static UIAudio I;

    [Header("General UI SFX")]
    public AudioClip hover;
    public AudioClip click;
    public AudioClip playButton;

    [Header("Inventory Drag/Drop SFX")]
    public AudioClip dragStart;
    public AudioClip dropSuccess;
    public AudioClip dropFail;

    [Header("Inventory Open/Close SFX")]
    public AudioClip inventoryOpen;
    public AudioClip inventoryClose;

    [Header("Potion Use SFX")]
    public AudioClip potionUse;
    public AudioClip potionFail;


    AudioSource src;

    void Awake()
    {

        Debug.Log("UIAudio instance: " + I);

        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D
    }

    // ----------------- Basic UI -----------------

    public void PlayHover()
    {
        if (hover) src.PlayOneShot(hover);
    }

    public void PlayClick()
    {
        if (click) src.PlayOneShot(click);
    }

    // ------------- Inventory Drag/Drop ----------

    public void PlayDragStart()
    {
        if (dragStart) src.PlayOneShot(dragStart);
    }

    public void PlayDropSuccess()
    {
        if (dropSuccess) src.PlayOneShot(dropSuccess);
    }

    public void PlayDropFail()
    {
        if (dropFail) src.PlayOneShot(dropFail);
    }

    // -------- Inventory Open / Close ------------

    public void PlayInventoryOpen()
    {
        if (inventoryOpen) src.PlayOneShot(inventoryOpen);
    }

    public void PlayInventoryClose()
    {
        if (inventoryClose) src.PlayOneShot(inventoryClose);
    }

    public void PlayPotionUse()
    {
        if (potionUse) src.PlayOneShot(potionUse);
    }

    public void PlayPotionFail()
    {
        if (potionFail) src.PlayOneShot(potionFail);
    }
    public void PlayPlayButton()
    {
        if (playButton) src.PlayOneShot(playButton);
    }


}
