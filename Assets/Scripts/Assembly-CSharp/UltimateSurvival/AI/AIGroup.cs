using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UltimateSurvival.AI
{
	public class AIGroup : MonoBehaviour
	{
		public enum SpawnMode
		{
			AtNight = 0,
			AtDaytime = 1,
			AllDay = 2
		}

		[SerializeField]
		private Color m_GroupColor;

		[SerializeField]
		private Color m_DetectColor;

		[SerializeField]
		[Space]
		private bool m_EnableSpawning = true;

		[SerializeField]
		private bool m_MakeAgentsChildren = true;

		[SerializeField]
		private GameObject[] m_Prefabs;

		[SerializeField]
		[Space]
		[Clamp(0f, 300f)]
		private float m_GroupRadius = 10f;

		[SerializeField]
		[Space]
		[Clamp(0f, 300f)]
		private float m_DetectRadius = 10f;

		[SerializeField]
		[Clamp(0f, 500f)]
		private int m_MaxCount = 3;

		[SerializeField]
		[Space]
		private SpawnMode m_SpawnMode;

		[SerializeField]
		[Clamp(3f, 120f)]
		private float m_SpawnInterval = 30f;

		private float m_LastUpdateTime;

		private List<Vector3> m_SpawnPoints = new List<Vector3>();

		private List<EntityEventHandler> m_AliveAgents = new List<EntityEventHandler>();

		private Transform m_Player;

		private int UpdateInterval = 5;

		private float timeTemp;

		private void Start()
		{
			for (int i = 0; i < m_MaxCount; i++)
			{
				Vector3 vector = base.transform.position + new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y) * m_GroupRadius;
				NavMeshHit hit;
				if (NavMesh.SamplePosition(vector, out hit, 10f, -1))
				{
					vector = hit.position;
				}
				m_SpawnPoints.Add(vector);
			}
			m_Player = GameController.LocalPlayer.transform;
		}

		private void Update()
		{
			if (m_Player != null)
			{
				bool flag = false;
				if (Time.time >= timeTemp + (float)UpdateInterval)
				{
					float num = Vector3.Distance(m_Player.position, base.transform.position);
					if (num < m_GroupRadius + m_DetectRadius)
					{
						if (m_AliveAgents.Count == m_MaxCount)
						{
							m_LastUpdateTime = Time.time + m_SpawnInterval;
						}
						flag = m_EnableSpawning && Time.time > m_LastUpdateTime && m_AliveAgents.Count < m_MaxCount && Vector3.Dot(m_Player.forward, base.transform.position - m_Player.position) < 0f;
					}
					else if (num > m_GroupRadius + m_DetectRadius + 10f)
					{
						RemoveAllAgents();
					}
					timeTemp = Time.time;
				}
				if (flag)
				{
					TrySpawn();
				}
			}
		}

		private void TrySpawn()
		{
			bool flag = m_SpawnMode == SpawnMode.AllDay;
			bool flag2 = MonoSingleton<TimeOfDay>.Instance.State.Get() == ET.TimeOfDay.Night && m_SpawnMode == SpawnMode.AtNight;
			bool flag3 = MonoSingleton<TimeOfDay>.Instance.State.Get() == ET.TimeOfDay.Day && m_SpawnMode == SpawnMode.AtDaytime;
			if (!flag && !flag2 && !flag3)
			{
				return;
			}
			m_LastUpdateTime = Time.time + m_SpawnInterval;
			Vector3 position = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];
			GameObject original = m_Prefabs[Random.Range(0, m_Prefabs.Length)];
			GameObject gameObject = Object.Instantiate(original, position, Quaternion.Euler(Vector3.up * Random.Range(-360f, 360f)));
			EntityEventHandler agent = gameObject.GetComponent<EntityEventHandler>();
			if (m_MakeAgentsChildren)
			{
				gameObject.transform.SetParent(base.transform, true);
			}
			if (agent != null)
			{
				m_AliveAgents.Add(agent);
				agent.Death.AddListener(delegate
				{
					On_AgentDeath(agent);
				});
			}
		}

		private void RemoveAllAgents()
		{
			m_AliveAgents.Clear();
			int childCount = base.transform.childCount;
			for (int num = childCount - 1; num >= 0; num--)
			{
				Object.Destroy(base.transform.GetChild(num).gameObject);
			}
		}

		private void On_AgentDeath(EntityEventHandler agent)
		{
			m_AliveAgents.Remove(agent);
		}

		private void OnDrawGizmosSelected()
		{
			Color color = Gizmos.color;
			Gizmos.color = m_GroupColor;
			Gizmos.DrawSphere(base.transform.position, m_GroupRadius);
			Gizmos.color = m_DetectColor;
			Gizmos.DrawSphere(base.transform.position, m_GroupRadius + m_DetectRadius);
			Gizmos.color = color;
		}
	}
}
