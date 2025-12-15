using System.Collections.Generic;
using System.Linq;
using UltimateSurvival.AI.Actions;
using UltimateSurvival.AI.Goals;
using UnityEngine;

namespace UltimateSurvival.AI
{
	[RequireComponent(typeof(AISettings))]
	public class AIBrain : AIBehaviour
	{
		[SerializeField]
		private List<Action> m_AvailableActions;

		[SerializeField]
		private List<Goal> m_AvailableGoals;

		[SerializeField]
		private Action m_Fallback;

		[SerializeField]
		private float m_MinPlanInterval;

		[SerializeField]
		private float m_MinGoalPriorityCheckInterval;

		private Queue<Action> m_ActionQueue;

		private Planner m_Planner;

		private Goal m_CurrentGoal;

		private StateData m_WorldState;

		private AISettings m_Settings;

		private float m_LastPlanTime;

		private float m_LastGoalPriorityCheckTime;

		public List<Action> AvailableActions
		{
			get
			{
				return m_AvailableActions;
			}
		}

		public Action Fallback
		{
			get
			{
				return m_Fallback;
			}
		}

		public Queue<Action> ActionQueue
		{
			get
			{
				return m_ActionQueue;
			}
		}

		public Goal CurrentGoal
		{
			get
			{
				return m_CurrentGoal;
			}
		}

		public StateData WorldState
		{
			get
			{
				return m_WorldState;
			}
		}

		public AISettings Settings
		{
			get
			{
				return m_Settings;
			}
		}

		private void Start()
		{
			m_Fallback = Object.Instantiate(m_Fallback);
			for (int i = 0; i < m_AvailableActions.Count; i++)
			{
				m_AvailableActions[i] = Object.Instantiate(m_AvailableActions[i]);
			}
			for (int j = 0; j < m_AvailableGoals.Count; j++)
			{
				m_AvailableGoals[j] = Object.Instantiate(m_AvailableGoals[j]);
			}
			m_Planner = new Planner();
			m_Settings = GetComponent<AISettings>();
			CreateNewWorldState();
			InitializeData();
		}

		private void Update()
		{
			bool flag = IsReplanNeededBecauseOfGoals();
			bool flag2 = m_ActionQueue == null || m_ActionQueue.Count == 0;
			if (flag2 || flag)
			{
				Replan();
			}
			if (flag2)
			{
				return;
			}
			Action action = m_ActionQueue.Peek();
			bool flag3 = Time.time - m_LastPlanTime > m_MinPlanInterval && action.IsInterruptable;
			if (m_ActionQueue.Count > 1 && flag3)
			{
				Action action2 = m_ActionQueue.ToArray()[1];
				bool flag4 = action2.Priority > action.Priority;
				bool flag5 = action2.CanActivate(this);
				if (flag4 && flag5)
				{
					m_LastPlanTime = Time.time;
					m_ActionQueue.Dequeue();
					action = m_ActionQueue.Peek();
				}
			}
			if (action.IsActive)
			{
				if (!action.StillValid(this))
				{
					action.IsActive = false;
					m_ActionQueue.Clear();
					return;
				}
				action.OnUpdate(this);
				if (action.IsDone(this))
				{
					action.OnCompletion(this);
					action.IsActive = false;
					m_ActionQueue.Dequeue();
				}
			}
			else if (!action.IsActive)
			{
				if (!action.CanActivate(this))
				{
					m_ActionQueue.Clear();
					return;
				}
				action.Activate(this);
				action.IsActive = true;
			}
		}

		private bool IsReplanNeededBecauseOfGoals()
		{
			if (m_AvailableGoals == null || m_AvailableGoals.Count == 0)
			{
				return false;
			}
			if (m_ActionQueue == null || m_ActionQueue.Count == 0)
			{
				return false;
			}
			bool result = false;
			if (Time.time - m_LastGoalPriorityCheckTime > m_MinGoalPriorityCheckInterval)
			{
				m_LastGoalPriorityCheckTime = Time.time;
				for (int i = 0; i < m_AvailableGoals.Count; i++)
				{
					m_AvailableGoals[i].RecalculatePriority(this);
				}
				m_AvailableGoals = m_AvailableGoals.OrderByDescending((Goal x) => x.Priority).ToList();
				if (m_AvailableGoals[0] != CurrentGoal)
				{
					m_ActionQueue.Peek().OnDeactivation(this);
					result = true;
				}
			}
			return result;
		}

		private void Replan()
		{
			for (int i = 0; i < m_AvailableActions.Count; i++)
			{
				m_AvailableActions[i].IsActive = false;
			}
			if (!m_Planner.Plan(m_AvailableGoals, m_AvailableActions, this, out m_ActionQueue, out m_CurrentGoal))
			{
				FallBack();
				return;
			}
			m_Fallback.OnDeactivation(this);
			m_Fallback.IsActive = false;
			m_LastPlanTime = Time.time;
		}

		private void FallBack()
		{
			if (m_ActionQueue == null || m_ActionQueue.Peek() != m_Fallback)
			{
				m_ActionQueue = new Queue<Action>();
				m_ActionQueue.Enqueue(m_Fallback);
				m_Fallback.Activate(this);
				m_Fallback.IsActive = true;
			}
		}

		private void CreateNewWorldState()
		{
			m_WorldState = new StateData();
			m_WorldState.Add("Is Player Dead", false);
			m_WorldState.Add("Can Attack Player", false);
			m_WorldState.Add("Player in sight", false);
			m_WorldState.Add("Next To Food", false);
			m_WorldState.Add("Is Hungry", false);
		}

		private void InitializeData()
		{
			if (m_AvailableActions.Count == 0)
			{
				Debug.LogError("No actions set for " + base.gameObject.name + " entity", this);
				return;
			}
			if (m_AvailableGoals.Count == 0)
			{
				Debug.LogError("No goals set for " + base.gameObject.name + " entity", this);
				return;
			}
			for (int i = 0; i < m_AvailableActions.Count; i++)
			{
				m_AvailableActions[i].OnStart(this);
				m_AvailableActions[i].IsActive = false;
			}
			for (int j = 0; j < m_AvailableGoals.Count; j++)
			{
				m_AvailableGoals[j].OnStart();
			}
			m_Fallback.OnStart(this);
			m_Fallback.IsActive = false;
		}
	}
}
