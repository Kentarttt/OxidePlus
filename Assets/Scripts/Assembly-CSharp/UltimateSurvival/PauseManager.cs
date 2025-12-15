using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace UltimateSurvival
{
	public class PauseManager : NetworkBehaviour
	{
		public GameObject mainCanvas;

		public GameObject pauseCanvas;

		public MouseLook mouseLook;

		public void SetPause(bool tf)
		{
			mouseLook.enabled = !tf;
			mainCanvas.SetActive(!tf);
			pauseCanvas.SetActive(tf);
		}

		public void Disconnect()
        {
			if (NetworkServer.active && NetworkClient.isConnected) NetworkManager.singleton.StopHost();
			else if (!NetworkServer.active && NetworkClient.isConnected) NetworkManager.singleton.StopClient();
			else if (NetworkServer.active && !NetworkClient.isConnected) NetworkManager.singleton.StopServer();
        }

		public void Exit() => Application.Quit();
	}
}
