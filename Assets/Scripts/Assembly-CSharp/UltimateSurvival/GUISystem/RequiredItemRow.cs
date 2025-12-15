using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class RequiredItemRow : MonoBehaviour
	{
		[SerializeField]
		private Color m_HaveEnoughColor = Color.white;

		[SerializeField]
		private Color m_DontHaveEnoughColor = Color.red;

		[SerializeField]
		private Text m_Amount;

		[SerializeField]
		private Text m_Type;

		[SerializeField]
		private Text m_Total;

		[SerializeField]
		private Text m_Have;

		public void Set(int amount, string type, int total, int have)
		{
			bool flag = !string.IsNullOrEmpty(type);
			m_Amount.text = ((!flag) ? string.Empty : amount.ToString());
			m_Type.text = ((!flag) ? string.Empty : type);
			m_Total.text = ((!flag) ? string.Empty : total.ToString());
			m_Have.text = ((!flag) ? string.Empty : have.ToString());
			if (flag)
			{
				m_Have.color = ((have < total) ? m_DontHaveEnoughColor : m_HaveEnoughColor);
			}
		}
	}
}
