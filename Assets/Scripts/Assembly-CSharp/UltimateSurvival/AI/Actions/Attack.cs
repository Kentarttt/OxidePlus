using UnityEngine;

namespace UltimateSurvival.AI.Actions
{
	public class Attack : Action
	{
		[SerializeField]
		private float m_MinAttackDistance = 1.5f;

		[SerializeField]
		private float m_AttackInterval = 0.8f;

		private bool m_HasKilledPlayer;

		private GameObject m_Target;

		private float m_NextTimeCanAttack;

		public override void OnStart(AIBrain brain)
		{
			m_Priority = 10;
			m_RepeatType = ET.ActionRepeatType.Single;
			m_IsInterruptable = false;
			base.Preconditions.Add("Can Attack Player", true);
			base.Effects.Add("Is Player Dead", true);
		}

		public override bool CanActivate(AIBrain brain)
		{
			return brain.Settings.Detection.HasTarget();
		}

		public override void Activate(AIBrain brain)
		{
			m_Target = brain.Settings.Detection.LastChasedTarget;
		}

		public override void OnUpdate(AIBrain brain)
		{
			if (Time.time > m_NextTimeCanAttack)
			{
				brain.Settings.Animation.SetTrigger("Attack");
				m_NextTimeCanAttack = Time.time + m_AttackInterval;
			}
			RotateTowards(brain.transform, m_Target.transform, 5f);
		}

		public override bool StillValid(AIBrain brain)
		{
			bool flag = Vector3.Distance(m_Target.transform.position, brain.transform.position) < m_MinAttackDistance;
			bool flag2 = Time.time > m_NextTimeCanAttack;
			return (brain.Settings.Detection.HasTarget() && flag) || !flag2;
		}

		public override bool IsDone(AIBrain brain)
		{
			return m_HasKilledPlayer && Time.time > m_NextTimeCanAttack;
		}

		private void RotateTowards(Transform transform, Transform target, float rotationSpeed)
		{
			Vector3 normalized = (target.position - transform.position).normalized;
			Quaternion b = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			transform.rotation = Quaternion.Slerp(transform.rotation, b, Time.deltaTime * rotationSpeed);
		}
	}
}
