using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class PoolManager : MonoBehaviour
	{
		[SerializeField]
		private List<Pool> _pools = new List<Pool>();

		private void Awake()
		{
			InitializePools();
		}

		private void InitializePools()
		{
			for (int i = 0; i < _pools.Count; i++)
			{
				Pool pool = _pools[i];
				if (pool.PoolName == string.Empty)
				{
					Debug.LogError("Pool with index of " + i + " does not contain a pool name.");
					break;
				}
				Transform transform = new GameObject(pool.PoolName + " Pool").transform;
				transform.SetParent(base.transform);
				for (int j = 0; j < pool.Amount; j++)
				{
					if (!pool.Prefab)
					{
						Debug.LogError("There is no prefab assigned for the " + pool.PoolName + " pool");
						return;
					}
					GameObject gameObject = Object.Instantiate(pool.Prefab);
					gameObject.transform.SetParent(transform);
					gameObject.SetActive(false);
					pool.PooledObjects.Add(gameObject);
				}
			}
		}

		public Pool GetPool(string name)
		{
			Pool pool = null;
			for (int i = 0; i < _pools.Count; i++)
			{
				if (_pools[i].PoolName == name)
				{
					pool = _pools[i];
				}
			}
			if (pool == null)
			{
				Debug.LogError("Couldn't find pool with name: " + name);
			}
			return pool;
		}
	}
}
