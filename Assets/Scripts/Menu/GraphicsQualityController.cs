using UnityEngine;
using UnityEngine.UI;

class GraphicsQualityController : MonoBehaviour
{
    [SerializeField] private Slider qualitySlider;
    [SerializeField] private Text qualityValueText;
    
    private void Awake()
    {
        qualitySlider.value = QualitySettings.GetQualityLevel();
        qualityValueText.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public void ChangeGraphicsQuality(float qualityLevel)
    {
        QualitySettings.SetQualityLevel((int)qualityLevel, true);
        PlayerPrefs.SetInt("GraphicsQuality", (int)qualityLevel);
        qualityValueText.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }
}