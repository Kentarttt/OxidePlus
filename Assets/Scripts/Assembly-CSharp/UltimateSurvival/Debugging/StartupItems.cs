using System;
using UnityEngine;

namespace UltimateSurvival.Debugging
{
	public class StartupItems : MonoBehaviour
	{
		[Serializable]
		public class ItemToAdd
		{
			[SerializeField]
			private string m_Name;

			[SerializeField]
			[Clamp(1f, 9999f)]
			private int m_Count = 1;

			public string Name
			{
				get
				{
					return m_Name;
				}
			}

			public int Count
			{
				get
				{
					return m_Count;
				}
			}
		}

		[SerializeField]
		[Reorderable]
		private ReorderableItemToAddList m_InventoryItems;

		[SerializeField]
		[Reorderable]
		private ReorderableItemToAddList m_HotbarItems;

		private void Start()
		{
			foreach (ItemToAdd inventoryItem in m_InventoryItems)
			{
				int added;
				MonoSingleton<InventoryController>.Instance.AddItemToCollection(inventoryItem.Name, inventoryItem.Count, "Inventory", out added);
			}
			foreach (ItemToAdd hotbarItem in m_HotbarItems)
			{
				int added2;
				MonoSingleton<InventoryController>.Instance.AddItemToCollection(hotbarItem.Name, hotbarItem.Count, "Hotbar", out added2);
			}
		}
	}
}
