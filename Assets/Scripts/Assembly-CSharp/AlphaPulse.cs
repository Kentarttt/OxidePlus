using UnityEngine;

public class AlphaPulse
{
	private bool m_IsLerping;

	private bool m_PulsingAtMax;

	private float m_PulseMin;

	private float m_PulseMax;

	private float m_LerpDuration;

	private float m_StartTime;

	private Color m_ToPulse;

	public AlphaPulse(Color tP, float min, float max)
	{
		m_ToPulse = tP;
		m_PulseMin = min;
		m_PulseMax = max;
	}

	public void StartPulse(float lerpDuration)
	{
		if (m_StartTime == 0f && !m_IsLerping)
		{
			m_LerpDuration = lerpDuration;
			m_StartTime = Time.time;
			m_IsLerping = true;
			if (m_ToPulse.a == m_PulseMax)
			{
				m_PulsingAtMax = false;
			}
			else if (m_ToPulse.a == m_PulseMin)
			{
				m_PulsingAtMax = true;
			}
		}
	}

	public float UpdatePulse()
	{
		if (!m_IsLerping)
		{
			return 0f;
		}
		float num = Time.time - m_StartTime;
		float t = num / m_LerpDuration;
		float num2 = ((!m_PulsingAtMax) ? m_PulseMin : m_PulseMax);
		m_ToPulse.a = Mathf.Lerp(m_ToPulse.a, num2, t);
		if (m_ToPulse.a == num2)
		{
			StopPulse();
		}
		return m_ToPulse.a;
	}

	private void StopPulse()
	{
		m_StartTime = 0f;
		m_IsLerping = false;
	}
}
