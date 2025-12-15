using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	public Slider sensSlider;

	public Text sensText;

	public Slider ddSlider;

	public Text ddText;

	public Slider tqSlider;

	public Text tqText;

	private void Start()
	{
		Application.targetFrameRate = 60;
		if (!PlayerPrefs.HasKey("tex_quality"))
		{
			PlayerPrefs.SetInt("draw_distance", 18);
			PlayerPrefs.SetInt("tex_quality", 4);
		}
		if (!PlayerPrefs.HasKey("sensitivity"))
		{
			PlayerPrefs.SetFloat("sensitivity", 1f);
		}
		UpdateValues();
	}

	public void UpdateValues()
	{
		float @float = PlayerPrefs.GetFloat("sensitivity");
		sensText.text = @float.ToString("F1");
		sensSlider.value = @float * 10f;
		int @int = PlayerPrefs.GetInt("draw_distance");
		ddText.text = (@int * 100).ToString();
		ddSlider.value = @int;
		int int2 = PlayerPrefs.GetInt("tex_quality");
		tqText.text = int2 * 25 + "%";
		tqSlider.value = int2;
		QualitySettings.globalTextureMipmapLimit = 4 - int2;
	}

	public void SetSensValue()
	{
		float value = sensSlider.value / 10f;
		sensText.text = value.ToString("F1");
		PlayerPrefs.SetFloat("sensitivity", value);
	}

	public void SetDDValue()
	{
		int num = (int)ddSlider.value;
		ddText.text = (num * 100).ToString();
		PlayerPrefs.SetInt("draw_distance", num);
	}

	public void SetTQValue()
	{
		int num = (int)tqSlider.value;
		tqText.text = num * 25 + "%";
		PlayerPrefs.SetInt("tex_quality", num);
		QualitySettings.globalTextureMipmapLimit = 4 - num;
	}
}
