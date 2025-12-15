using UnityEngine;
using UnityEngine.UI;

class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text valueText;
    [SerializeField] private int multiplyDisplay = 100;
    [SerializeField] private float minValue = 1.0f;
    [SerializeField] private float maxValue = 2.0f;
    [SerializeField] private bool showPercentage = false;
    [SerializeField] private bool wholeNumbers = false;
    [SerializeField] private bool isInt = true;
    [SerializeField] private string saveName;

    private bool autoSave = false;
    private string playerPrefsKey = "MySliderValue";

    private void Start()
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        float savedValue = PlayerPrefs.GetFloat(playerPrefsKey + saveName, slider.value);
        slider.value = savedValue;
        UpdateValueText(savedValue);
    }

    public void UpdateValueText(float value)
    {
        float valText = value * multiplyDisplay;

        if (isInt)
        {
            
            if (showPercentage) valueText.text = Mathf.RoundToInt(valText) + "%";
            else valueText.text = Mathf.RoundToInt(valText).ToString();
        }
        else
        {
            if (showPercentage) valueText.text = valText.ToString("F1") + "%";
            else valueText.text = valText.ToString("F1");
        }


        if (autoSave) Save(value);
        else autoSave = true;
    }

    private void Save(float value)
    {
        PlayerPrefs.SetFloat(playerPrefsKey + saveName, value);
        PlayerPrefs.Save();
    }
}