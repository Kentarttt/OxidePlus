using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.AI
{
	[Serializable]
	public class EntityDetection
	{
		[SerializeField]
		[Tooltip("Time it takes to look for targets again.")]
		private float m_TargetSearchDelay;

		[SerializeField]
		private Transform m_Eyes;

		[SerializeField]
		[Range(0f, 360f)]
		[Tooltip("Angle at which it can spot a player.")]
		private int m_ViewAngle = 120;

		[SerializeField]
		[Tooltip("Radius around the AI at which it can spot a player")]
		private int m_ViewRadius = 3;

		[SerializeField]
		[Clamp(0f, 300f)]
		private int m_HearRange = 50;

		[SerializeField]
		[Tooltip("Used for finding only specific types of targets.")]
		private LayerMask m_SpotMask;

		[SerializeField]
		[Tooltip("Used to know what objects can be blocking our view from the AI.")]
		private LayerMask m_ObstacleMask;

		private List<GameObject> m_VisibleTargets = new List<GameObject>();

		private List<GameObject> m_StillInRangeTargets = new List<GameObject>();

		private Transform m_Transform;

		private float m_LastTargetFindTime;

		public int ViewRadius
		{
			get
			{
				return m_ViewRadius;
			}
		}

		public int ViewAngle
		{
			get
			{
				return m_ViewAngle;
			}
		}

		public int HearRange
		{
			get
			{
				return m_HearRange;
			}
		}

		public GameObject LastChasedTarget { get; set; }

		public List<GameObject> VisibleTargets
		{
			get
			{
				return m_VisibleTargets;
			}
		}

		public List<GameObject> StillInRangeTargets
		{
			get
			{
				return m_StillInRangeTargets;
			}
		}

		public void Initialize(Transform transform)
		{
			m_Transform = transform;
		}

		public void Update(AIBrain brain)
		{
			if (m_TargetSearchDelay != 0f && Time.time - m_LastTargetFindTime >= m_TargetSearchDelay)
			{
				m_VisibleTargets = GetVisibleTargets();
				GetTargetsStillInRange();
				m_LastTargetFindTime = Time.time;
				StateData.OverrideValue("Player in sight", HasTarget(), brain.WorldState);
			}
		}

		public bool HasTarget()
		{
			return m_StillInRangeTargets.Count > 0;
		}

		public bool HasVisibleTarget()
		{
			return m_VisibleTargets.Count > 0;
		}

		public Transform GetRandomTarget()
		{
			return m_StillInRangeTargets[UnityEngine.Random.Range(0, m_StillInRangeTargets.Count)].transform;
		}

		private void GetTargetsStillInRange()
		{
			Collider[] collidersInRange = GetCollidersInRange();
			for (int i = 0; i < collidersInRange.Length; i++)
			{
				if (m_VisibleTargets.Contains(collidersInRange[i].gameObject) && !m_StillInRangeTargets.Contains(collidersInRange[i].gameObject))
				{
					m_StillInRangeTargets.Add(collidersInRange[i].gameObject);
				}
			}
			for (int j = 0; j < m_StillInRangeTargets.Count; j++)
			{
				bool flag = false;
				for (int k = 0; k < collidersInRange.Length; k++)
				{
					if (collidersInRange[k].gameObject == m_StillInRangeTargets[j])
					{
						flag = true;
					}
				}
				if (!flag)
				{
					m_StillInRangeTargets.Remove(m_StillInRangeTargets[j]);
				}
			}
		}

		private List<GameObject> GetVisibleTargets()
		{
			List<GameObject> list = new List<GameObject>();
			Collider[] collidersInRange = GetCollidersInRange();
			for (int i = 0; i < collidersInRange.Length; i++)
			{
				if (collidersInRange[i].GetComponent<PlayerEventHandler>() == null)
				{
					continue;
				}
				Transform transform = collidersInRange[i].transform;
				Vector3 vector = transform.position + Vector3.up;
				Vector3 normalized = (vector - m_Eyes.position).normalized;
				if (Vector3.Angle(m_Eyes.forward, normalized) < (float)(m_ViewAngle / 2))
				{
					float maxDistance = Vector3.Distance(m_Eyes.position, vector);
					if (!Physics.Raycast(m_Eyes.position, normalized, maxDistance, m_ObstacleMask))
					{
						list.Add(transform.gameObject);
					}
				}
			}
			return list;
		}

		private Collider[] GetCollidersInRange()
		{
			return Physics.OverlapSphere(m_Transform.position, m_ViewRadius, m_SpotMask);
		}
	}
}
