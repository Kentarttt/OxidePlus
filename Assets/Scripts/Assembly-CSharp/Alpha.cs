using UnityEngine;
using UnityEngine.UI;

public class Alpha : MonoBehaviour
{
	public Text versionText;

	private void Start()
	{
		if ((bool)versionText)
		{
			TextAsset textAsset = Resources.Load<TextAsset>("build");
			if ((bool)textAsset) versionText.text = textAsset.text;
		}
	}
}
