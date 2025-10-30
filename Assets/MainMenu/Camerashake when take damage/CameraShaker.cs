using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] private float defaultDuration = 0.12f;
    [SerializeField] private float defaultAmplitude = 0.08f;  
    [SerializeField] private float frequency = 22f;            

    Vector3 originalLocalPos;
    Quaternion originalLocalRot;
    Coroutine shakeCo;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
    }

    public void Shake(float amplitude, float duration)
    {
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(
            amplitude <= 0f ? defaultAmplitude : amplitude,
            duration <= 0f ? defaultDuration : duration
        ));
    }

    IEnumerator ShakeRoutine(float amplitude, float duration)
    {
        float t = 0f;
       
        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        float seedZ = Random.value * 100f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float decay = 1f - Mathf.Clamp01(t / duration); 

            float nx = (Mathf.PerlinNoise(seedX, Time.unscaledTime * frequency) * 2f - 1f);
            float ny = (Mathf.PerlinNoise(seedY, Time.unscaledTime * frequency) * 2f - 1f);
            float nz = (Mathf.PerlinNoise(seedZ, Time.unscaledTime * frequency) * 2f - 1f);

            
            Vector3 offset = new Vector3(nx, ny * 0.6f, nz * 0.4f) * (amplitude * decay);
            transform.localPosition = originalLocalPos + offset;

            
            float rotAmt = (amplitude * 3f) * decay; 
            Quaternion rot =
                Quaternion.AngleAxis(ny * rotAmt, Vector3.right) *
                Quaternion.AngleAxis(nx * rotAmt, Vector3.up);
            transform.localRotation = originalLocalRot * rot;

            yield return null;
        }

       
        transform.localPosition = originalLocalPos;
        transform.localRotation = originalLocalRot;
        shakeCo = null;
    }
}
