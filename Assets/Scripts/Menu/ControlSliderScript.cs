using UnityEngine;
using UnityEngine.UI;

class ControlSliderScript : MonoBehaviour
{
    public static ControlSliderScript instance;

    [SerializeField] private Slider slider;
    [SerializeField] private Text valueText;
    [SerializeField] private float minValue = 1.0f;
    [SerializeField] private float maxValue = 2.0f;
    [SerializeField] private bool wholeNumbers = false;
    [SerializeField] private bool autoSave = true;
    [SerializeField] private string saveName;

    private string playerPrefsKey = "MySliderValue";

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

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
        valueText.text = Mathf.RoundToInt(value * 100) + "%";
        if (autoSave) PlayerPrefs.SetFloat(playerPrefsKey + saveName, value);
    }

    public void SaveValue(float value)
    {
        slider.value = value;
        valueText.text = Mathf.RoundToInt(value * 100) + "%";
        PlayerPrefs.SetFloat(playerPrefsKey + saveName, value);
        ButtonScaler.instance.UpdateButtonSizes(value);
    }
}