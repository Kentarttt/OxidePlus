using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class ItemDatabase : ScriptableObject
	{
		[SerializeField]
		private ItemCategory[] m_Categories;

		[SerializeField]
		private ItemProperty.Definition[] m_ItemProperties;

		public ItemCategory[] Categories
		{
			get
			{
				return m_Categories;
			}
		}

		public bool FindItemById(int id, out ItemData itemData)
		{
			for (int i = 0; i < m_Categories.Length; i++)
			{
				ItemCategory itemCategory = m_Categories[i];
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					if (itemCategory.Items[j].Id == id)
					{
						itemData = itemCategory.Items[j];
						return true;
					}
				}
			}
			itemData = null;
			return false;
		}

		public bool FindItemByName(string name, out ItemData itemData)
		{
			for (int i = 0; i < m_Categories.Length; i++)
			{
				ItemCategory itemCategory = m_Categories[i];
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					if (itemCategory.Items[j].Name == name)
					{
						itemData = itemCategory.Items[j];
						return true;
					}
				}
			}
			itemData = null;
			return false;
		}

		public bool FindRecipeById(int id, out Recipe recipe)
		{
			for (int i = 0; i < m_Categories.Length; i++)
			{
				ItemCategory itemCategory = m_Categories[i];
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					if (itemCategory.Items[j].Id == id)
					{
						recipe = itemCategory.Items[j].Recipe;
						return true;
					}
				}
			}
			recipe = null;
			return false;
		}

		public bool FindRecipeByName(string name, out Recipe recipe)
		{
			for (int i = 0; i < m_Categories.Length; i++)
			{
				ItemCategory itemCategory = m_Categories[i];
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					if (itemCategory.Items[j].Name == name && itemCategory.Items[j].IsCraftable)
					{
						recipe = itemCategory.Items[j].Recipe;
						return true;
					}
				}
			}
			recipe = null;
			return false;
		}

		public List<string> GetAllItemNames()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < m_Categories.Length; i++)
			{
				ItemCategory itemCategory = m_Categories[i];
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					list.Add(itemCategory.Items[j].Name);
				}
			}
			return list;
		}

		public int GetItemCount()
		{
			int num = 0;
			for (int i = 0; i < m_Categories.Length; i++)
			{
				num += m_Categories[i].Items.Length;
			}
			return num;
		}

		private void OnValidate()
		{
			int num = 0;
			ItemCategory[] categories = m_Categories;
			foreach (ItemCategory itemCategory in categories)
			{
				for (int j = 0; j < itemCategory.Items.Length; j++)
				{
					itemCategory.Items[j].Id = num;
					itemCategory.Items[j].Category = itemCategory.Name;
					num++;
				}
			}
		}
	}
}
