using System;
using UnityEngine;

namespace UltimateSurvival.AI.Actions
{
	[Serializable]
	public class Action : ScriptableObject
	{
		protected int m_Priority;

		protected bool m_IsInterruptable;

		protected ET.ActionRepeatType m_RepeatType;

		private bool m_IsActive;

		private StateData m_Preconditions = new StateData();

		private StateData m_Effects = new StateData();

		public bool IsActive
		{
			get
			{
				return m_IsActive;
			}
			set
			{
				m_IsActive = value;
			}
		}

		public int Priority
		{
			get
			{
				return m_Priority;
			}
		}

		public bool IsInterruptable
		{
			get
			{
				return m_IsInterruptable;
			}
		}

		public ET.ActionRepeatType RepeatType
		{
			get
			{
				return m_RepeatType;
			}
		}

		public StateData Preconditions
		{
			get
			{
				return m_Preconditions;
			}
		}

		public StateData Effects
		{
			get
			{
				return m_Effects;
			}
		}

		public virtual void OnStart(AIBrain agent)
		{
		}

		public virtual void OnUpdate(AIBrain agent)
		{
		}

		public virtual void OnCompletion(AIBrain agent)
		{
			if (m_RepeatType != ET.ActionRepeatType.Repetitive)
			{
				StateData.OverrideValues(m_Effects, agent.WorldState);
			}
		}

		public virtual bool CanActivate(AIBrain brain)
		{
			return true;
		}

		public virtual bool StillValid(AIBrain brain)
		{
			return true;
		}

		public virtual void Activate(AIBrain brain)
		{
		}

		public virtual void OnDeactivation(AIBrain brain)
		{
		}

		public virtual bool IsDone(AIBrain brain)
		{
			return false;
		}

		public virtual void ResetValues()
		{
		}
	}
}
