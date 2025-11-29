using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFader : MonoBehaviour
{
    public static SceneFader I;

    [SerializeField] private float duration = 0.5f;
    private CanvasGroup cg;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        StartCoroutine(Fade(1f, 0f, setBlockers: false));   
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return Fade(cg.alpha, 1f, setBlockers: true); 
        var op = SceneManager.LoadSceneAsync((sceneName));
        while (!op.isDone) yield return null;
        yield return Fade(1f, 0f, setBlockers: false);      
    }

    private IEnumerator Fade(float from, float to, bool setBlockers)
    {
        cg.interactable = setBlockers;
        cg.blocksRaycasts = setBlockers;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;

        
        bool block = (to > 0.001f);
        cg.interactable = block && setBlockers;
        cg.blocksRaycasts = block && setBlockers;
    }
}
