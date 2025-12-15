using UnityEngine;

class SetQuality : MonoBehaviour
{
    private void Start()
    {
        if (PlayerPrefs.HasKey("GraphicsQuality")) QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("GraphicsQuality"));
        else PlayerPrefs.SetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
    }
}