using System;
using UnityEngine;
using UnityEngine.AI;

namespace UltimateSurvival.AI
{
	[Serializable]
	public class EntityMovement
	{
		[SerializeField]
		[Tooltip("Normal speed the agent will use.")]
		private float m_WalkSpeed;

		[Tooltip("Speed the agent will only use whenever an action requires it to hurry.")]
		[SerializeField]
		private float m_RunSpeed;

		private Vector3 m_CurrentDestination;

		private ET.AIMovementState m_MovementState;

		private AIBrain m_Brain;

		private NavMeshAgent m_Agent;

		public Vector3 CurrentDestination
		{
			get
			{
				return m_CurrentDestination;
			}
		}

		public ET.AIMovementState MovementState
		{
			get
			{
				return m_MovementState;
			}
		}

		public NavMeshAgent Agent
		{
			get
			{
				return m_Agent;
			}
		}

		public void Initialize(AIBrain brain)
		{
			m_Brain = brain;
			m_Agent = m_Brain.GetComponent<NavMeshAgent>();
		}

		public void Update(Transform transform)
		{
			Vector3 vector = m_Agent.nextPosition - transform.position;
			if (vector.magnitude > m_Agent.radius)
			{
				m_Agent.nextPosition = transform.position + 0.9f * vector;
			}
		}

		public NavMeshPath MoveTo(Vector3 position, bool fastMove = false)
		{
			NavMeshPath result = new NavMeshPath();
			m_Agent.SetDestination(position);
			m_CurrentDestination = position;
			bool flag = fastMove && m_Brain.Settings.Animation.ParameterExists("Run");
			bool flag2 = m_Brain.Settings.Animation.ParameterExists("Walk");
			if (flag)
			{
				ChangeMovementState(m_RunSpeed, "Run", true, ET.AIMovementState.Running);
			}
			else if (flag2)
			{
				ChangeMovementState(m_WalkSpeed, "Walk", true, ET.AIMovementState.Walking);
			}
			return result;
		}

		private void ChangeMovementState(float speed, string animName, bool animValue, ET.AIMovementState newState)
		{
			m_Agent.speed = speed;
			m_Brain.Settings.Animation.ToggleBool(animName, animValue);
			m_MovementState = newState;
		}

		public bool ReachedDestination(bool isStop = true)
		{
			if (m_Agent.remainingDistance <= m_Agent.stoppingDistance)
			{
				if (isStop)
				{
					string paramName = ((m_MovementState != ET.AIMovementState.Running) ? "Walk" : "Run");
					m_Brain.Settings.Animation.ToggleBool(paramName, false);
					m_MovementState = ET.AIMovementState.Idle;
				}
				return true;
			}
			return false;
		}
	}
}
