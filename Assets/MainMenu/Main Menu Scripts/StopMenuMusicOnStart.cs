using UnityEngine;

public class StopMenuMusicOnStart : MonoBehaviour
{
    void Start()
    {
        var music = FindObjectOfType<MenuMusic>();
        if (music) Destroy(music.gameObject);
    }
}
