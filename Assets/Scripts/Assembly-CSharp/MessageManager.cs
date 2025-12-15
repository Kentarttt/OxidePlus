using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
	public Text title;

	public Text description;

	public GameObject message;

	public void ShowMessage(string t, string d)
	{
		title.text = t;
		description.text = d;
		message.SetActive(true);
	}

	public void HideMessage()
	{
		title.text = "-";
		description.text = "-";
		message.SetActive(false);
	}
}
