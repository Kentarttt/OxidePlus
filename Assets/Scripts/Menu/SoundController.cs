using UnityEngine;
using UnityEngine.UI;

class SoundController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumeText;

    private string playerPrefsKey = "MySliderValue";

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(playerPrefsKey + "MasterVolume", 1f);
        UpdateVolume(volumeSlider.value);
    }

    public void UpdateVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(playerPrefsKey + "MasterVolume", value);
        PlayerPrefs.Save();
        UpdateVolumeText();
    }

    private void UpdateVolumeText() => volumeText.text = Mathf.RoundToInt(AudioListener.volume * 100) + "%"; 
}