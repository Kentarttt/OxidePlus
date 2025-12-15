using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

class ButtonScaler : MonoBehaviour
{
    public enum ScaleMode { Multiply, Add }

    public static ButtonScaler instance;

    [Header("UI Elements")]
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private List<RectTransform> buttonsToScale;

    [Header("Scaling Settings")]
    [SerializeField] private ScaleMode scaleMode = ScaleMode.Multiply;
    [SerializeField] private float scaleMultiplier = 1f;
    [SerializeField] private float scaleOffset = 0f;
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

    private void Start() => UpdateButtonSizes(PlayerPrefs.GetFloat(playerPrefsKey + saveName, 1.0f));

    public void UpdateButtonSizes(float value)
    {
        float scaleValue = 1f;
        switch (scaleMode)
        {
            case ScaleMode.Multiply:
                scaleValue = value * scaleMultiplier;
                break;
            case ScaleMode.Add:
                scaleValue = value + scaleOffset;
                break;
        }

        foreach (RectTransform button in buttonsToScale)
        {
            if (button != null)
            {
                button.localScale = new Vector3(scaleValue, scaleValue, 1);
            }
        }

        ControlSliderScript.instance.UpdateValueText(value);
    }
}