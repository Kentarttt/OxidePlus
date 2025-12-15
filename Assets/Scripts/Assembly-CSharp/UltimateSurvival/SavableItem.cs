using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class SavableItem
	{
		public Message<ItemProperty.Value> PropertyChanged = new Message<ItemProperty.Value>();

		public Message StackChanged = new Message();

		[SerializeField]
		private int m_CurrentInStack;

		[SerializeField]
		private List<ItemProperty.Value> m_CurrentPropertyValues;

		public bool Initialized { get; private set; }

		public ItemData ItemData { get; private set; }

		public int Id
		{
			get
			{
				return ItemData.Id;
			}
		}

		public string Name
		{
			get
			{
				return ItemData.Name;
			}
		}

		public int CurrentInStack
		{
			get
			{
				return m_CurrentInStack;
			}
			set
			{
				m_CurrentInStack = value;
				StackChanged.Send();
			}
		}

		public List<ItemProperty.Value> CurrentPropertyValues
		{
			get
			{
				return m_CurrentPropertyValues;
			}
		}

		public SavableItem(ItemData data, int currentInStack = 1, List<ItemProperty.Value> customPropertyValues = null)
		{
			CurrentInStack = Mathf.Clamp(currentInStack, 1, data.StackSize);
			if (customPropertyValues != null)
			{
				m_CurrentPropertyValues = CloneProperties(customPropertyValues);
			}
			else
			{
				m_CurrentPropertyValues = CloneProperties(data.PropertyValues);
			}
			ItemData = data;
			Initialized = true;
			for (int i = 0; i < m_CurrentPropertyValues.Count; i++)
			{
				m_CurrentPropertyValues[i].Changed.AddListener(On_PropertyChanged);
			}
		}

		public static implicit operator bool(SavableItem item)
		{
			return item != null;
		}

		public void OnLoad(ItemDatabase itemDatabase)
		{
			if ((bool)itemDatabase)
			{
				ItemData itemData;
				if (itemDatabase.FindItemById(Id, out itemData))
				{
					ItemData = itemData;
					Initialized = true;
					for (int i = 0; i < m_CurrentPropertyValues.Count; i++)
					{
						m_CurrentPropertyValues[i].Changed.AddListener(On_PropertyChanged);
					}
				}
				else
				{
					Debug.LogErrorFormat("[SavableItem] - This item couldn't be initialized and will not function properly. No item with the name {0} was found in the database!", Name);
				}
			}
			else
			{
				Debug.LogError("[SavableItem] - This item couldn't be initialized and will not function properly. The item database provided is null!");
			}
		}

		public string GetDescription(int index)
		{
			string result = string.Empty;
			if (index > -1 && ItemData.Descriptions.Length > index)
			{
				try
				{
					result = string.Format(ItemData.Descriptions[index], m_CurrentPropertyValues.ToArray());
				}
				catch
				{
					Debug.LogError("[SavableItem] - You tried to access a property through the item description, but the property doesn't exist. The item name is: " + Name);
				}
			}
			return result;
		}

		public bool HasProperty(string name)
		{
			if (!Initialized)
			{
				Debug.LogError("[SavableItem] - This SavableItem is not initialized, probably it was loaded and not initialized! (call OnLoad() after loading / deserializing).");
				return false;
			}
			for (int i = 0; i < m_CurrentPropertyValues.Count; i++)
			{
				if (m_CurrentPropertyValues[i].Name == name)
				{
					return true;
				}
			}
			return false;
		}

		public ItemProperty.Value GetPropertyValue(string name)
		{
			ItemProperty.Value result = null;
			if (!Initialized)
			{
				Debug.LogError("[SavableItem] - This SavableItem is not initialized, probably it was loaded and not initialized! (call OnLoad() after loading / deserializing).");
				return null;
			}
			for (int i = 0; i < m_CurrentPropertyValues.Count; i++)
			{
				if (m_CurrentPropertyValues[i].Name == name)
				{
					result = m_CurrentPropertyValues[i];
					break;
				}
			}
			return result;
		}

		public bool FindPropertyValue(string name, out ItemProperty.Value propertyValue)
		{
			propertyValue = null;
			if (!Initialized)
			{
				Debug.LogError("[SavableItem] - This SavableItem is not initialized, probably it was loaded and not initialized! (call OnLoad() after loading / deserializing).");
				return false;
			}
			for (int i = 0; i < m_CurrentPropertyValues.Count; i++)
			{
				if (m_CurrentPropertyValues[i].Name == name)
				{
					propertyValue = m_CurrentPropertyValues[i];
					return true;
				}
			}
			return false;
		}

		private List<ItemProperty.Value> CloneProperties(List<ItemProperty.Value> properties)
		{
			List<ItemProperty.Value> list = new List<ItemProperty.Value>();
			for (int i = 0; i < properties.Count; i++)
			{
				list.Add(properties[i].GetClone());
			}
			return list;
		}

		private void On_PropertyChanged(ItemProperty.Value propertyValue)
		{
			PropertyChanged.Send(propertyValue);
		}
	}
}
