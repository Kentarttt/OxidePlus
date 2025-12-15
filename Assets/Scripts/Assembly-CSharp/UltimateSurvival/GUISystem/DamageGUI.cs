using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class DamageGUI : GUIBehaviour
	{
		[Header("Blood Screen")]
		[SerializeField]
		private ImageFader m_BloodScreenFader;

		[Header("Damage Indicator")]
		[SerializeField]
		private RectTransform m_IndicatorRT;

		[SerializeField]
		private ImageFader m_IndicatorFader;

		[SerializeField]
		[Clamp(0f, 512f)]
		[Tooltip("Damage indicator distance (in pixels) from the screen center.")]
		private int m_IndicatorDistance = 128;

		private Vector3 m_LastHitPoint;

		private void Start()
		{
			base.Player.ChangeHealth.AddListener(OnSuccess_ChangeHealth);
		}

		private void Update()
		{
			if (m_IndicatorFader.Fading)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(base.Player.LookDirection.Get(), Vector3.up).normalized;
				Vector3 normalized2 = Vector3.ProjectOnPlane(m_LastHitPoint - base.Player.transform.position, Vector3.up).normalized;
				Vector3 lhs = Vector3.Cross(normalized, Vector3.up);
				float num = Vector3.Angle(normalized, normalized2) * Mathf.Sign(Vector3.Dot(lhs, normalized2));
				m_IndicatorRT.localEulerAngles = Vector3.forward * num;
				m_IndicatorRT.localPosition = m_IndicatorRT.up * m_IndicatorDistance;
			}
		}

		private void OnSuccess_ChangeHealth(HealthEventData healthEventData)
		{
			if (healthEventData.Delta < 0f)
			{
				m_BloodScreenFader.DoFadeCycle(this, healthEventData.Delta / 100f);
				if (healthEventData.HitPoint != Vector3.zero)
				{
					m_LastHitPoint = healthEventData.HitPoint;
					m_IndicatorFader.DoFadeCycle(this, 1f);
				}
			}
		}
	}
}
