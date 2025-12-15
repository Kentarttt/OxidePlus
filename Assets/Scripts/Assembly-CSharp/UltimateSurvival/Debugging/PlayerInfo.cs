using UnityEngine;

namespace UltimateSurvival.Debugging
{
	public class PlayerInfo : MonoBehaviour
	{
		private PlayerEventHandler m_Player;

		private bool m_Toggle;

		private void Start()
		{
			m_Player = GameController.LocalPlayer.GetComponent<PlayerEventHandler>();
		}

		private void OnGUI()
		{
			m_Toggle = GUILayout.Toggle(m_Toggle, "Player Info");
			if (m_Toggle)
			{
				GUILayout.Label("Velocity: " + m_Player.Velocity.Get());
				GUILayout.Label("Grounded: " + m_Player.IsGrounded.Get());
				GUILayout.Label("Jumping: " + m_Player.Jump.Active);
				GUILayout.Label("Walking: " + m_Player.Walk.Active);
				GUILayout.Label("Sprinting: " + m_Player.Run.Active);
				GUILayout.Label("Crouching: " + m_Player.Crouch.Active);
				GUILayout.Label("Is Close To An Object: " + m_Player.IsCloseToAnObject.Get());
			}
			Time.timeScale = GUI.HorizontalSlider(new Rect(16f, 64f, 64f, 16f), Time.timeScale, 0f, 1f);
			Time.fixedDeltaTime = 0.02f * Time.timeScale;
		}
	}
}
