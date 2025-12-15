using UnityEngine;
using UnityEngine.UI;

class ToggleScript : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private string saveName;
    [SerializeField] private int isOn = 1;
    private string playerPrefsKey = "MyBoolValue";

    private void Start()
    {
        int savedValue = PlayerPrefs.GetInt(playerPrefsKey + saveName, isOn);

        if (savedValue != 1) toggle.isOn = false;
        else if (savedValue == 1) toggle.isOn = true;
    }

    public void Save(bool value)
    {
        int val = 0;
        if (value) val = 1;
        else if (!value) val = 0;
        PlayerPrefs.SetInt(playerPrefsKey + saveName, val);
        PlayerPrefs.Save();
    }
}