using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class CraftingList : MonoBehaviour
	{
		public Message RecipesGenerated = new Message();

		[SerializeField]
		private CraftingMenu m_CraftingMenu;

		[SerializeField]
		private RectTransform m_ListTransform;

		[SerializeField]
		private RecipeSlot m_RecipeTemplate;

		private List<ObjectHolder> m_RecipesByCategory = new List<ObjectHolder>();

		public List<ObjectHolder> RecipesByCategory
		{
			get
			{
				return m_RecipesByCategory;
			}
		}

		private void Awake()
		{
			m_CraftingMenu.SelectedCategory.AddChangeListener(OnChanged_SelectedCategory);
			m_CraftingMenu.SelectedCategory.AddChangeListener(OnChanged_Category);
		}

		private void Start()
		{
			GenerateRecipes();
			m_ListTransform.anchoredPosition = new Vector2(m_ListTransform.anchoredPosition.x, 0f);
		}

		private void OnChanged_SelectedCategory()
		{
			m_ListTransform.anchoredPosition = new Vector2(m_ListTransform.anchoredPosition.x, 0f);
		}

		private void OnChanged_Category()
		{
			CraftingCategory craftingCategory = m_CraftingMenu.SelectedCategory.Get();
			foreach (ObjectHolder item in m_RecipesByCategory)
			{
				item.ActivateObjects(craftingCategory.HasCategory(item.Name));
			}
		}

		private void GenerateRecipes()
		{
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			m_RecipeTemplate.gameObject.SetActive(true);
			ItemCategory[] categories = database.Categories;
			foreach (ItemCategory itemCategory in categories)
			{
				List<GameObject> list = new List<GameObject>();
				ObjectHolder item = new ObjectHolder(itemCategory.Name, list);
				m_RecipesByCategory.Add(item);
				ItemData[] items = itemCategory.Items;
				foreach (ItemData itemData in items)
				{
					if (itemData.IsCraftable)
					{
						Recipe recipe = itemData.Recipe;
						RecipeSlot component = Object.Instantiate(m_RecipeTemplate.gameObject, m_ListTransform.position, m_ListTransform.rotation, m_ListTransform).GetComponent<RecipeSlot>();
						component.name = string.Format("Recipe Slot ({0})", itemData.Name);
						component.ShowRecipeForItem(itemData);
						list.Add(component.gameObject);
					}
				}
			}
			m_RecipeTemplate.gameObject.SetActive(false);
			RecipesGenerated.Send();
		}
	}
}
