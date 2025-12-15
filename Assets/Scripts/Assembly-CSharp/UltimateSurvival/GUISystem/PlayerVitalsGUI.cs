using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class PlayerVitalsGUI : GUIBehaviour
	{
		[SerializeField]
		private RectTransform m_HealthBar;

		[SerializeField]
		private RectTransform m_ThirstBar;

		[SerializeField]
		private RectTransform m_HungerBar;

		private void Start()
		{
			base.Player.Health.AddChangeListener(OnChanged_Health);
			base.Player.Thirst.AddChangeListener(OnChanged_Thirst);
			base.Player.Hunger.AddChangeListener(OnChanged_Hunger);
		}

		private void OnChanged_Health()
		{
			m_HealthBar.sizeDelta = new Vector2(base.Player.Health.Get(), m_HealthBar.sizeDelta.y);
		}

		private void OnChanged_Thirst()
		{
			m_ThirstBar.sizeDelta = new Vector2(base.Player.Thirst.Get(), m_ThirstBar.sizeDelta.y);
		}

		private void OnChanged_Hunger()
		{
			m_HungerBar.sizeDelta = new Vector2(base.Player.Hunger.Get(), m_HungerBar.sizeDelta.y);
		}
	}
}
