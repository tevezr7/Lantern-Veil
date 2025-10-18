using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    private static MenuMusic inst;

    void Awake()
    {
        if (inst != null) { Destroy(gameObject); return; }
        inst = this;
        DontDestroyOnLoad(gameObject);
    }
}
