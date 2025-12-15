using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class DynamicCrosshair : MonoBehaviour
	{
		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_Distance = 32f;

		[Header("Crosshair Parts")]
		[SerializeField]
		private Image m_Left;

		[SerializeField]
		private Image m_Right;

		[SerializeField]
		private Image m_Down;

		[SerializeField]
		private Image m_Up;

		public float Distance
		{
			get
			{
				return m_Distance;
			}
		}

		public void SetActive(bool active)
		{
			Image left = m_Left;
			bool flag = active;
			m_Up.enabled = flag;
			flag = flag;
			m_Down.enabled = flag;
			flag = flag;
			m_Right.enabled = flag;
			left.enabled = flag;
		}

		public void SetDistance(float distance)
		{
			m_Left.rectTransform.anchoredPosition = new Vector2(0f - distance, 0f);
			m_Right.rectTransform.anchoredPosition = new Vector2(distance, 0f);
			m_Down.rectTransform.anchoredPosition = new Vector2(0f, 0f - distance);
			m_Up.rectTransform.anchoredPosition = new Vector2(0f, distance);
			m_Distance = distance;
		}

		public void SetColor(Color color)
		{
			Image left = m_Left;
			Color color2 = color;
			m_Up.color = color2;
			color2 = color2;
			m_Down.color = color2;
			color2 = color2;
			m_Right.color = color2;
			left.color = color2;
		}

		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				SetDistance(m_Distance);
			}
		}
	}
}
