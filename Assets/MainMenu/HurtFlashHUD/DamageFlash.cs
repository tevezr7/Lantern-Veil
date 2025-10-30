using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Image overlay;   

    [Header("Flash Settings")]
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.35f; 
    [SerializeField] private float fadeInTime = 0.05f; 
    [SerializeField] private float fadeOutTime = 0.25f;  
    [SerializeField] private bool useUnscaledTime = false; 

    Color baseColor;
    Coroutine flashCo;

    void Awake()
    {
        if (!overlay) overlay = GetComponent<Image>();
        baseColor = overlay ? overlay.color : new Color(1, 0, 0, 0);
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (!overlay) return;
        var c = overlay.color;
        c.a = Mathf.Clamp01(a);
        overlay.color = c;
    }

    float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    public void Hit(float intensity01 = 1f)
    {
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(FlashRoutine(Mathf.Clamp01(intensity01)));
    }

    IEnumerator FlashRoutine(float intensity01)
    {
        
        float t = 0f;
        float target = maxAlpha * intensity01;
        while (t < fadeInTime)
        {
            t += DeltaTime;
            SetAlpha(Mathf.Lerp(overlay.color.a, target, t / fadeInTime));
            yield return null;
        }
        SetAlpha(target);

       
        t = 0f;
        while (t < fadeOutTime)
        {
            t += DeltaTime;
            SetAlpha(Mathf.Lerp(target, 0f, t / fadeOutTime));
            yield return null;
        }
        SetAlpha(0f);
        flashCo = null;
    }
}
