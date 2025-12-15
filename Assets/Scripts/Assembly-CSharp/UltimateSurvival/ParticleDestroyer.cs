using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class ParticleDestroyer : MonoBehaviour
	{
		[SerializeField]
		private float m_MinDuration = 8f;

		[SerializeField]
		private float m_MaxDuration = 10f;

		private float m_MaxLifetime;

		private bool m_EarlyStop;

		private IEnumerator Start()
		{
			ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = systems;
			foreach (ParticleSystem particleSystem in array)
			{
				m_MaxLifetime = Mathf.Max(particleSystem.main.startLifetimeMultiplier, m_MaxLifetime);
			}
			float stopTime = Time.time + Random.Range(m_MinDuration, m_MaxDuration);
			while (Time.time < stopTime || m_EarlyStop)
			{
				yield return null;
			}
			ParticleSystem[] array2 = systems;
			foreach (ParticleSystem particleSystem2 in array2)
			{
				ParticleSystem.EmissionModule emission = particleSystem2.emission;
				emission.enabled = false;
			}
			yield return new WaitForSeconds(m_MaxLifetime);
			Object.Destroy(base.gameObject);
		}

		public void Stop()
		{
			m_EarlyStop = true;
		}
	}
}
