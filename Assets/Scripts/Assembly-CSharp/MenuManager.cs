using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	[SerializeField] private Text[] username;
	[SerializeField] private InputField inputUsername;
	[SerializeField] private UnityEvent autoLogin;

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
		{
			for (int i = 0; i < username.Length; i++)
				username[i].text = PlayerPrefs.GetString("username", "Username");

			autoLogin.Invoke();
        }
    }

	public void OpenScene(string scene) => SceneManager.LoadScene(scene, LoadSceneMode.Single);

	public void OpenURL(string url) => Application.OpenURL(url);


	public void Exit() => Application.Quit();

	public void SetUsername()
	{
		PlayerPrefs.SetString("username", inputUsername.text);
        for (int i = 0; i < username.Length; i++) 
		username[i].text = inputUsername.text;
    }
}