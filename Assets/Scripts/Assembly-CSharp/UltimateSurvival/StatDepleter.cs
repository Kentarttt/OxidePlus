using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class StatDepleter
	{
		[SerializeField]
		[Clamp(0f, 100f)]
		private float m_DepletionRate = 1f;

		[SerializeField]
		[Clamp(0f, 100f)]
		[Tooltip("Damage applied when this stat reaches 0.")]
		private float m_Damage = 3f;

		[SerializeField]
		[Clamp(0f, 100f)]
		[Tooltip("How frequent damage will be applied.")]
		private float m_DamagePeriod = 3f;

		private float m_LastDamageTime;

		public void Update(Value<float> statValue, Attempt<HealthEventData> healthChanger)
		{
			float value = statValue.Get() - m_DepletionRate * Time.deltaTime;
			value = Mathf.Clamp(value, 0f, 100f);
			statValue.Set(value);
			if (statValue.Is(0f) && Time.time - m_LastDamageTime > m_DamagePeriod)
			{
				m_LastDamageTime = Time.time;
				healthChanger.Try(new HealthEventData(0f - m_Damage));
			}
		}
	}
}
