using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class RecipeInspector : MonoBehaviour
	{
		[Header("Setup")]
		[SerializeField]
		private ItemContainer m_Inventory;

		[SerializeField]
		private CraftingList m_CraftingList;

		[SerializeField]
		private Window m_Window;

		[SerializeField]
		private int m_MaxCraftAmount = 999;

		[Header("GUI Elements")]
		[SerializeField]
		private Text m_ItemName;

		[SerializeField]
		private Text m_Description;

		[SerializeField]
		private Image m_Icon;

		[SerializeField]
		private RequiredItemRow[] m_RequiredItemsTable;

		[SerializeField]
		private Text m_TotalTime;

		[SerializeField]
		private Text m_DesiredAmount;

		private ItemData m_InspectedItem;

		private int m_CurrentDesiredAmount;

		public RecipeSlot InspectedSlot { get; private set; }

		public RecipeSlot FirstSlot { get; private set; }

		public void Try_StartCrafting()
		{
			bool flag = m_CurrentDesiredAmount > 0;
			RequiredItem[] requiredItems = m_InspectedItem.Recipe.RequiredItems;
			foreach (RequiredItem requiredItem in requiredItems)
			{
				if (requiredItem.Amount * m_CurrentDesiredAmount > m_Inventory.GetItemCount(requiredItem.Name))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				CraftData craftData = new CraftData();
				craftData.Result = m_InspectedItem;
				craftData.Amount = m_CurrentDesiredAmount;
				CraftData arg = craftData;
				if (MonoSingleton<InventoryController>.Instance.CraftItem.Try(arg))
				{
					m_CurrentDesiredAmount = 1;
					ShowRecipeInfo(m_InspectedItem);
				}
			}
		}

		public void TryIncreaseAmount()
		{
			if (m_InspectedItem != null)
			{
				m_CurrentDesiredAmount++;
				ShowRecipeInfo(m_InspectedItem);
			}
		}

		public void TryDecreaseAmount()
		{
			if (m_InspectedItem != null)
			{
				m_CurrentDesiredAmount = Mathf.Clamp(m_CurrentDesiredAmount - 1, 1, m_MaxCraftAmount);
				ShowRecipeInfo(m_InspectedItem);
			}
		}

		private void Awake()
		{
			if (!m_CraftingList)
			{
				Debug.LogError("Please assign the Crafting List in the inspector!", this);
			}
			m_CraftingList.RecipesGenerated.AddListener(On_RecipesGenerated);
			m_TotalTime.text = string.Empty;
		}

		private void On_RecipesGenerated()
		{
			bool flag = true;
			foreach (ObjectHolder item in m_CraftingList.RecipesByCategory)
			{
				foreach (GameObject @object in item.ObjectList)
				{
					RecipeSlot component = @object.GetComponent<RecipeSlot>();
					if ((bool)component)
					{
						component.PointerUp += On_Slot_PointerUp;
						if (flag)
						{
							FirstSlot = component;
							flag = false;
						}
					}
				}
			}
		}

		private void On_Slot_PointerUp(BaseEventData data, RecipeSlot slot)
		{
			if (EventSystem.current.currentSelectedGameObject == slot.gameObject)
			{
				if ((bool)m_Window)
				{
					m_Window.Open();
				}
				InspectedSlot = slot;
				m_InspectedItem = slot.Result;
				m_CurrentDesiredAmount = 1;
				ShowRecipeInfo(m_InspectedItem);
				slot.E_Deselect += On_Slot_Deselect;
			}
			else
			{
				On_Slot_Deselect(data, slot);
			}
		}

		private void ShowRecipeInfo(ItemData itemData)
		{
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			m_ItemName.text = ((!(itemData.DisplayName == string.Empty)) ? itemData.DisplayName : itemData.Name);
			m_Description.text = itemData.Descriptions[0];
			m_Icon.sprite = itemData.Icon;
			for (int i = 0; i < m_RequiredItemsTable.Length; i++)
			{
				RequiredItemRow requiredItemRow = m_RequiredItemsTable[i];
				if (itemData.Recipe.RequiredItems.Length > i)
				{
					RequiredItem requiredItem = itemData.Recipe.RequiredItems[i];
					int total = requiredItem.Amount * Mathf.Clamp(m_CurrentDesiredAmount, 1, int.MaxValue);
					requiredItemRow.Set(requiredItem.Amount, requiredItem.Name, total, m_Inventory.GetItemCount(requiredItem.Name));
				}
				else
				{
					requiredItemRow.Set(0, string.Empty, 0, 0);
				}
			}
			m_TotalTime.text = string.Format("ETA: {0}s", (itemData.Recipe.Duration * m_CurrentDesiredAmount).ToString());
			m_DesiredAmount.text = ((m_CurrentDesiredAmount <= 0) ? string.Empty : "x") + m_CurrentDesiredAmount;
		}

		private void On_Slot_Deselect(BaseEventData data, RecipeSlot deselectedSlot)
		{
			StartCoroutine(C_CheckNextSelection());
		}

		private IEnumerator C_CheckNextSelection()
		{
			yield return null;
			RecipeSlot selectedSlot = null;
			GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
			if ((bool)currentSelected)
			{
				selectedSlot = currentSelected.GetComponent<RecipeSlot>();
			}
			if (!currentSelected || !selectedSlot)
			{
				if ((bool)m_Window)
				{
					m_Window.Close();
				}
				InspectedSlot = null;
			}
		}

		public void ShowFirstRecipe()
		{
			if ((bool)m_Window)
			{
				m_Window.Open();
			}
			InspectedSlot = FirstSlot;
			m_InspectedItem = FirstSlot.Result;
			m_CurrentDesiredAmount = 1;
			ShowRecipeInfo(m_InspectedItem);
		}

		public void Close()
		{
			StartCoroutine(C_CheckNextSelection());
		}
	}
}
