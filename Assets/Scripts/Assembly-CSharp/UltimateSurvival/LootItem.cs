using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class LootItem
	{
		[SerializeField]
		private string m_ItemName;

		[SerializeField]
		[Range(0f, 100f)]
		private float m_SpawnChance;

		[SerializeField]
		private int m_MinAmount;

		[SerializeField]
		private int m_MaxAmount;

		public string ItemName
		{
			get
			{
				return m_ItemName;
			}
		}

		public float SpawnChance
		{
			get
			{
				return m_SpawnChance;
			}
		}

		public void AddToInventory(out int added, float amountFactor)
		{
			added = 0;
			int num = Mathf.CeilToInt((float)UnityEngine.Random.Range(m_MinAmount, m_MaxAmount) * amountFactor);
			if (num > 0)
			{
				MonoSingleton<InventoryController>.Instance.AddItemToCollection(m_ItemName, num, "Inventory", out added);
			}
		}

		public GameObject CreatePickup(Vector3 position, Quaternion rotation)
		{
			ItemData itemData;
			if (MonoSingleton<InventoryController>.Instance.Database.FindItemByName(m_ItemName, out itemData) && (bool)itemData.WorldObject)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(itemData.WorldObject, position, rotation);
				ItemPickup component = gameObject.GetComponent<ItemPickup>();
				if ((bool)component)
				{
					component.ItemToAdd.CurrentInStack = UnityEngine.Random.Range(m_MinAmount, m_MaxAmount);
				}
				return gameObject;
			}
			return null;
		}
	}
}
