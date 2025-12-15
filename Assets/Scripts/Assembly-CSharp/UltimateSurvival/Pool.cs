using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class Pool
	{
		[SerializeField]
		private string m_PoolName;

		[SerializeField]
		private GameObject m_Prefab;

		[SerializeField]
		private int m_Amount;

		private List<GameObject> m_PooledObjects = new List<GameObject>();

		public string PoolName
		{
			get
			{
				return m_PoolName;
			}
		}

		public GameObject Prefab
		{
			get
			{
				return m_Prefab;
			}
		}

		public int Amount
		{
			get
			{
				return m_Amount;
			}
		}

		public List<GameObject> PooledObjects
		{
			get
			{
				return m_PooledObjects;
			}
		}

		private GameObject FindFirstActiveObject()
		{
			GameObject gameObject = null;
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				if (m_PooledObjects[i].activeInHierarchy)
				{
					gameObject = m_PooledObjects[i];
				}
			}
			if (gameObject == null)
			{
				Debug.LogError("There are no available active objects to get from the " + m_PoolName + " pool");
			}
			return gameObject;
		}

		private GameObject FindFirstInactiveObject()
		{
			GameObject gameObject = null;
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				if (!m_PooledObjects[i].activeInHierarchy)
				{
					gameObject = m_PooledObjects[i];
				}
			}
			if (gameObject == null)
			{
				Debug.LogError("There are no available inactive objects to get from the " + m_PoolName + " pool");
			}
			return gameObject;
		}

		public void Spawn(Vector3 position, Quaternion rotation)
		{
			GameObject gameObject = FindFirstInactiveObject();
			Transform transform = gameObject.transform;
			transform.position = position;
			transform.rotation = rotation;
			gameObject.SetActive(true);
			gameObject.GetComponent<PoolableObject>().OnSpawn();
		}

		public void SpawnAll(Vector3[] positions, Quaternion[] rotations)
		{
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				Spawn(positions[i], rotations[i]);
			}
		}

		public void DespawnSpecificObject(GameObject toDespawn)
		{
			if (m_PooledObjects.Contains(toDespawn))
			{
				toDespawn.SetActive(false);
			}
			else
			{
				Debug.LogError("Object that entered the trigger is not part of the " + m_PoolName + " pool");
			}
		}

		public void Despawn()
		{
			GameObject gameObject = FindFirstActiveObject();
			gameObject.GetComponent<PoolableObject>().OnDespawn();
			gameObject.SetActive(false);
		}

		public void DestroyObjects(bool activeOnes)
		{
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				if (m_PooledObjects[i].activeInHierarchy == activeOnes)
				{
					m_PooledObjects[i].GetComponent<PoolableObject>().OnPoolableDestroy();
					UnityEngine.Object.Destroy(m_PooledObjects[i]);
				}
			}
		}

		public void DestroyAll()
		{
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				m_PooledObjects[i].GetComponent<PoolableObject>().OnPoolableDestroy();
				UnityEngine.Object.Destroy(m_PooledObjects[i]);
			}
		}

		public void DespawnAll()
		{
			for (int i = 0; i < m_PooledObjects.Count; i++)
			{
				m_PooledObjects[i].GetComponent<PoolableObject>().OnDespawn();
				m_PooledObjects[i].SetActive(false);
			}
		}
	}
}
