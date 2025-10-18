using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private const string MixerParam = "MasterVolume";
    private const string VolKey = "settings.masterVolume";
    private const string FullKey = "settings.fullscreen";

    private void Awake()
    {
        float vol = PlayerPrefs.GetFloat(VolKey, 0.75f);
        bool fs = PlayerPrefs.GetInt(FullKey, Screen.fullScreen ? 1 : 0) == 1;

        if (masterVolumeSlider) masterVolumeSlider.SetValueWithoutNotify(vol);
        if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(fs);

        ApplyVolume(vol);
        Screen.fullScreen = fs;

        if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider) masterVolumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        if (fullscreenToggle) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }

    private void OnVolumeChanged(float linear)
    {
        ApplyVolume(linear);
        PlayerPrefs.SetFloat(VolKey, linear);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt(FullKey, isFull ? 1 : 0);
        PlayerPrefs.Save();
        StartCoroutine(RefreshUILayoutNextFrame());
    }

    private System.Collections.IEnumerator RefreshUILayoutNextFrame()
    {
        yield return null; 
        Canvas.ForceUpdateCanvases();

       
        var optionsRoot = GetComponent<RectTransform>();
        if (optionsRoot)
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionsRoot);
    }

    private void ApplyVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat(MixerParam, dB);
    }
}
