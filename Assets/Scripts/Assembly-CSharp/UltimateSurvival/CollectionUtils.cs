using System.Collections.Generic;
using UltimateSurvival.GUISystem;

namespace UltimateSurvival
{
	public static class CollectionUtils
	{
		public static void AddItem(ItemData itemData, int amount, List<ItemHolder> itemHolders, out int added, List<ItemProperty.Value> customPropertyValues = null)
		{
			added = 0;
			for (int i = 0; i < itemHolders.Count; i++)
			{
				int added2;
				itemHolders[i].TryAddItem(itemData, amount, out added2, customPropertyValues);
				added += added2;
				if (added == amount)
				{
					break;
				}
			}
		}

		public static void AddItem(ItemData itemData, int amount, List<ItemHolder> itemHolders, List<ItemProperty.Value> customPropertyValues = null)
		{
			int num = 0;
			for (int i = 0; i < itemHolders.Count; i++)
			{
				int added;
				itemHolders[i].TryAddItem(itemData, amount, out added, customPropertyValues);
				num += added;
				amount -= added;
				if (amount == 0)
				{
					break;
				}
			}
		}

		public static void AddItem(ItemData itemData, int amount, List<Slot> slots, out int added, List<ItemProperty.Value> customPropertyValues = null)
		{
			added = 0;
			for (int i = 0; i < slots.Count; i++)
			{
				int added2;
				slots[i].ItemHolder.TryAddItem(itemData, amount, out added2, customPropertyValues);
				added += added2;
				amount -= added2;
				if (amount == 0)
				{
					break;
				}
			}
		}

		public static void AddItem(ItemData itemData, int amount, Slot slot, out int added, List<ItemProperty.Value> customPropertyValues = null)
		{
			added = 0;
			int added2;
			slot.ItemHolder.TryAddItem(itemData, amount, out added2, customPropertyValues);
			added += added2;
			amount -= added2;
			if (amount != 0)
			{
			}
		}

		public static void RemoveItems(string itemName, int amount, List<ItemHolder> holders, out int removed)
		{
			removed = 0;
			for (int i = 0; i < holders.Count; i++)
			{
				if (holders[i].HasItem && holders[i].CurrentItem.Name == itemName)
				{
					int removed2;
					holders[i].RemoveFromStack(amount - removed, out removed2);
					removed += removed2;
					if (removed == amount)
					{
						break;
					}
				}
			}
		}

		public static void RemoveItems(string itemName, int amount, List<Slot> slots, out int removed)
		{
			removed = 0;
			for (int i = 0; i < slots.Count; i++)
			{
				if (slots[i].HasItem && slots[i].CurrentItem.Name == itemName)
				{
					int removed2;
					slots[i].ItemHolder.RemoveFromStack(amount - removed, out removed2);
					removed += removed2;
					if (removed == amount)
					{
						break;
					}
				}
			}
		}
	}
}
