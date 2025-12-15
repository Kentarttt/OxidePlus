using System;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	[Serializable]
	public class DurabilityBar
	{
		[SerializeField]
		private GameObject m_Background;

		[SerializeField]
		private Image m_Bar;

		[SerializeField]
		private Gradient m_ColorGradient;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Durability;

		private bool m_Active = true;

		public bool Active
		{
			get
			{
				return m_Active;
			}
		}

		public void SetActive(bool active)
		{
			if ((bool)m_Background)
			{
				m_Background.SetActive(active);
			}
			m_Bar.enabled = active;
			m_Active = active;
		}

		public void SetFillAmount(float fillAmount)
		{
			m_Bar.color = m_ColorGradient.Evaluate(fillAmount);
			m_Bar.fillAmount = fillAmount;
		}
	}
}
