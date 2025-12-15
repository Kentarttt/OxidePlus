using UnityEngine;

public class ControlSave : MonoBehaviour
{
    private RectTransform rectTransform;
    private string saveKey;
    private string playerPrefsKey = "MySliderValue";

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        float savedValue = PlayerPrefs.GetFloat(playerPrefsKey + "Size", 1.0f);

        saveKey = gameObject.name + "_Pos";
        float x = PlayerPrefs.GetFloat(saveKey + "_x", rectTransform.anchoredPosition.x);
        float y = PlayerPrefs.GetFloat(saveKey + "_y", rectTransform.anchoredPosition.y);
        rectTransform.anchoredPosition = new Vector2(x, y);
        rectTransform.localScale = new Vector3(savedValue, savedValue, 1);
    }
}
