using UnityEngine;

namespace UltimateSurvival
{
	public class DamageArea : MonoBehaviour
	{
		[SerializeField]
		private Vector2 m_DamagePerSecond = new Vector2(3f, 5f);

		public bool Active { get; set; }

		private void OnTriggerStay(Collider other)
		{
			if (Active)
			{
				EntityEventHandler component = other.GetComponent<EntityEventHandler>();
				if ((bool)component)
				{
					HealthEventData arg = new HealthEventData((0f - Random.Range(m_DamagePerSecond.x, m_DamagePerSecond.y)) * Time.deltaTime);
					component.ChangeHealth.Try(arg);
				}
			}
		}
	}
}
