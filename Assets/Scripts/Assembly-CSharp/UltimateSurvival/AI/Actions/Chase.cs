using UnityEngine;
using UnityEngine.AI;

namespace UltimateSurvival.AI.Actions
{
	public class Chase : Action
	{
		[SerializeField]
		private SoundPlayer m_InitialScreamAudio;

		[SerializeField]
		private SoundPlayer m_ScreamingAudio;

		[SerializeField]
		private Vector2 m_ScreamInterval = new Vector2(0.7f, 1.2f);

		private Transform m_Target;

		private float m_NextScreamTime;

		public override void OnStart(AIBrain brain)
		{
			m_Priority = 1;
			m_IsInterruptable = false;
			m_RepeatType = ET.ActionRepeatType.Single;
			base.Preconditions.Add("Player in sight", true);
			base.Effects.Add("Can Attack Player", true);
			ResetValues();
		}

		public override bool CanActivate(AIBrain brain)
		{
			if (!brain.Settings.Detection.HasTarget())
			{
				return false;
			}
			m_Target = brain.Settings.Detection.GetRandomTarget();
			brain.Settings.Detection.LastChasedTarget = m_Target.gameObject;
			return true;
		}

		public override void Activate(AIBrain brain)
		{
			brain.Settings.Movement.MoveTo(m_Target.position, true);
			if (Vector3.Distance(brain.transform.position, m_Target.position) > 3.5f)
			{
				m_InitialScreamAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, brain.Settings.AudioSource);
			}
		}

		public override void OnUpdate(AIBrain brain)
		{
			brain.Settings.Movement.MoveTo(m_Target.position, true);
			NavMeshAgent agent = brain.Settings.Movement.Agent;
			if (agent.velocity.sqrMagnitude < 0.01f)
			{
				brain.Settings.Animation.ToggleBool("Run", false);
				agent.updateRotation = false;
				RotateTowards(agent.transform, m_Target, 5f);
				return;
			}
			agent.updateRotation = true;
			brain.Settings.Animation.ToggleBool("Run", true);
			if (Time.time > m_NextScreamTime)
			{
				m_ScreamingAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, brain.Settings.AudioSource);
				m_NextScreamTime = Time.time + Random.Range(m_ScreamInterval.x, m_ScreamInterval.y);
			}
		}

		public override bool StillValid(AIBrain brain)
		{
			bool flag = brain.Settings.Detection.HasTarget();
			if (!flag)
			{
				brain.Settings.Animation.ToggleBool("Run", false);
				brain.Settings.Movement.Agent.updateRotation = true;
			}
			return flag;
		}

		public override bool IsDone(AIBrain brain)
		{
			return brain.Settings.Movement.ReachedDestination() && (brain.transform.position - m_Target.position).sqrMagnitude < brain.Settings.Movement.Agent.stoppingDistance * brain.Settings.Movement.Agent.stoppingDistance;
		}

		public override void ResetValues()
		{
			m_Target = null;
		}

		private void RotateTowards(Transform transform, Transform target, float rotationSpeed)
		{
			Vector3 normalized = (target.position - transform.position).normalized;
			Quaternion b = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			transform.rotation = Quaternion.Slerp(transform.rotation, b, Time.deltaTime * rotationSpeed);
		}
	}
}
