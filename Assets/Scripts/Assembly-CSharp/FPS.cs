using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.5f;

    private Text fpsText;
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps;
    private string playerPrefsKey = "MyBoolValue";

    private void Start()
    {
        if (PlayerPrefs.GetInt(playerPrefsKey + "FPS", 1) == 0) gameObject.SetActive(false);
        timeleft = updateInterval;
        fpsText = GetComponent<Text>();
    }

    private void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            fps = (accum / frames);
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }

        if (fps >= 45f) fpsText.color = new Color(0, 1, 0, 1);
        else if (fps < 45f && fps >= 25f) fpsText.color = new Color(1, 1, 0, 1);
        else if (fps < 25f) fpsText.color = new Color(1, 0, 0, 1);

        fpsText.text = fps.ToString("F1") + " FPS";
    }
}