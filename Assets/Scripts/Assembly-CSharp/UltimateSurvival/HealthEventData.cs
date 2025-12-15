using UnityEngine;

namespace UltimateSurvival
{
	public class HealthEventData
	{
		public float Delta { get; set; }

		public EntityEventHandler Damager { get; private set; }

		public Vector3 HitPoint { get; private set; }

		public Vector3 HitDirection { get; private set; }

		public float HitImpulse { get; private set; }

		public Collider AffectedCollider { get; private set; }

		public HealthEventData(float delta, EntityEventHandler damager = null, Vector3 hitPoint = default(Vector3), Vector3 hitDirection = default(Vector3), float hitImpulse = 0f)
		{
			Delta = delta;
			Damager = damager;
			HitPoint = hitPoint;
			HitDirection = hitDirection;
			HitImpulse = hitImpulse;
		}
	}
}
