using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.StandardAssets
{
	[RequireComponent(typeof(Text))]
	public class FPSDisplayer : MonoBehaviour
	{
		private const float fpsMeasurePeriod = 0.5f;

		private int m_FpsAccumulator;

		private float m_FpsNextPeriod;

		private int m_CurrentFps;

		private const string display = "{0} FPS";

		private Text m_Text;

		private void Start()
		{
			m_FpsNextPeriod = Time.realtimeSinceStartup + 0.5f;
			m_Text = GetComponent<Text>();
		}

		private void Update()
		{
			m_FpsAccumulator++;
			if (Time.realtimeSinceStartup > m_FpsNextPeriod)
			{
				m_CurrentFps = (int)((float)m_FpsAccumulator / 0.5f);
				m_FpsAccumulator = 0;
				m_FpsNextPeriod += 0.5f;
				m_Text.text = string.Format("{0} FPS", m_CurrentFps);
				if (m_CurrentFps < 30)
				{
					m_Text.color = Color.yellow;
				}
				else if (m_CurrentFps < 10)
				{
					m_Text.color = Color.red;
				}
				else
				{
					m_Text.color = Color.green;
				}
			}
		}
	}
}
