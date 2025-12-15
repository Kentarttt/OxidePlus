using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class CannibalSpawner : MonoBehaviour
	{
		public enum SpawnMode
		{
			AtNight = 0,
			AtDaytime = 1,
			AllDay = 2
		}

		[SerializeField]
		private bool m_ShowGUI = true;

		[SerializeField]
		private bool m_EnableSpawning = true;

		[SerializeField]
		[Space]
		[Clamp(0f, 99f)]
		private int m_MaxCount = 20;

		[SerializeField]
		private SpawnMode m_SpawnMode;

		[SerializeField]
		[Clamp(3f, 120f)]
		private float m_SpawnInterval = 30f;

		[SerializeField]
		private GameObject[] m_Prefabs;

		private float m_LastUpdateTime;

		private GameObject[] m_SpawnPoints;

		private List<EntityEventHandler> m_AliveCannibals = new List<EntityEventHandler>();

		private void Start()
		{
			m_SpawnPoints = GameObject.FindGameObjectsWithTag("Spawn Point");
		}

		private void OnGUI()
		{
			if (!m_ShowGUI)
			{
				return;
			}
			Rect position = new Rect(8f, 64f, 256f, 20f);
			GUI.Label(position, "# CANNIBAL SPAWNER");
			position.width = 128f;
			position.y = position.yMax + 4f;
			m_EnableSpawning = GUI.Toggle(position, m_EnableSpawning, "Enable Spawning");
			position.y = position.yMax + 4f;
			GUI.Label(position, "Spawn Interval: " + m_SpawnInterval);
			position.y = position.yMax + 4f;
			m_SpawnInterval = (int)GUI.HorizontalSlider(position, m_SpawnInterval, 3f, 120f);
			position.y = position.yMax + 4f;
			GUI.Label(position, "Max Count: " + m_MaxCount);
			position.y = position.yMax + 4f;
			m_MaxCount = (int)GUI.HorizontalSlider(position, m_MaxCount, 0f, 99f);
			position.y = position.yMax + 4f;
			if (!GUI.Button(position, "Remove All"))
			{
				return;
			}
			AIEventHandler[] array = Object.FindObjectsOfType<AIEventHandler>();
			foreach (AIEventHandler aIEventHandler in array)
			{
				if (aIEventHandler.name.Contains("Cannibal"))
				{
					Object.Destroy(aIEventHandler.gameObject);
				}
			}
			m_AliveCannibals.Clear();
		}

		private void Update()
		{
			if (m_EnableSpawning && Time.time > m_LastUpdateTime && m_AliveCannibals.Count < m_MaxCount)
			{
				bool flag = m_SpawnMode == SpawnMode.AllDay;
				bool flag2 = MonoSingleton<TimeOfDay>.Instance.State.Get() == ET.TimeOfDay.Night && m_SpawnMode == SpawnMode.AtNight;
				bool flag3 = MonoSingleton<TimeOfDay>.Instance.State.Get() == ET.TimeOfDay.Day && m_SpawnMode == SpawnMode.AtDaytime;
				if (flag || flag2 || flag3)
				{
					m_LastUpdateTime = Time.time + m_SpawnInterval;
					Spawn();
				}
			}
		}

		private void Spawn()
		{
			Transform transform = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Length)].transform;
			GameObject original = m_Prefabs[Random.Range(0, m_Prefabs.Length)];
			GameObject gameObject = Object.Instantiate(original, transform.position, Quaternion.Euler(Vector3.up * Random.Range(-360f, 360f)));
			EntityEventHandler cannibal = gameObject.GetComponent<EntityEventHandler>();
			if (cannibal != null)
			{
				m_AliveCannibals.Add(cannibal);
				cannibal.Death.AddListener(delegate
				{
					On_CannibalDeath(cannibal);
				});
			}
		}

		private void On_CannibalDeath(EntityEventHandler cannibal)
		{
			m_AliveCannibals.Remove(cannibal);
		}
	}
}
