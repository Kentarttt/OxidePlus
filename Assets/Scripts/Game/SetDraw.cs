using UnityEngine;

class SetDraw : MonoBehaviour
{
    public string saveName;
    private string playerPrefsKey = "MySliderValue";
    private Camera Camera;

    private void Awake()
    {
        float draw = PlayerPrefs.GetFloat(playerPrefsKey + saveName, 2000f);
        float fov = PlayerPrefs.GetFloat(playerPrefsKey + "FOV", 60f);
        Camera = GetComponent<Camera>();
        Camera.farClipPlane = draw;
        Camera.fieldOfView = fov;
    }
}