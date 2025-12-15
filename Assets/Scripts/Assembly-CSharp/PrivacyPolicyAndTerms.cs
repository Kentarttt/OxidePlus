using UnityEngine;

public class PrivacyPolicyAndTerms : MonoBehaviour
{
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void AcceptTerms()
	{
		PlayerPrefs.SetInt("terms_accepted", 1);
	}

	public void Exit()
	{
		Application.Quit();
	}
}
