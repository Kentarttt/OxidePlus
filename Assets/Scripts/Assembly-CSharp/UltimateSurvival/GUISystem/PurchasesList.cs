using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class PurchasesList : MonoBehaviour
	{
		public Message RecipesGenerated = new Message();

		[SerializeField]
		private PurchasesMenu m_PurchasesMenu;

		[SerializeField]
		private RectTransform m_ListTransform;

		[SerializeField]
		private PurchaseSlot m_PurchaseTemplate;

		private List<ShopObjectHolder> m_ItemsByCategory = new List<ShopObjectHolder>();

		public List<ShopObjectHolder> ItemsByCategory
		{
			get
			{
				return m_ItemsByCategory;
			}
		}

		private void Awake()
		{
			m_PurchasesMenu.SelectedCategory.AddChangeListener(OnChanged_SelectedCategory);
			m_PurchasesMenu.SelectedCategory.AddChangeListener(OnChanged_Category);
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
			CraftingCategory craftingCategory = m_PurchasesMenu.SelectedCategory.Get();
			foreach (ShopObjectHolder item in m_ItemsByCategory)
			{
				item.ActivateObjects(craftingCategory.HasCategory(item.Name));
			}
		}

		private void GenerateRecipes()
		{
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			m_PurchaseTemplate.gameObject.SetActive(true);
			ItemCategory[] categories = database.Categories;
			foreach (ItemCategory itemCategory in categories)
			{
				List<GameObject> list = new List<GameObject>();
				ShopObjectHolder item = new ShopObjectHolder(itemCategory.Name, list);
				m_ItemsByCategory.Add(item);
				ItemData[] items = itemCategory.Items;
				foreach (ItemData itemData in items)
				{
					if (itemData.Purchase.Price > 0)
					{
						Recipe recipe = itemData.Recipe;
						PurchaseSlot component = Object.Instantiate(m_PurchaseTemplate.gameObject, m_ListTransform.position, m_ListTransform.rotation, m_ListTransform).GetComponent<PurchaseSlot>();
						component.name = string.Format("Purchase Slot ({0})", itemData.Name);
						component.ShowPurchaseItem(itemData);
						list.Add(component.gameObject);
					}
				}
			}
			m_PurchaseTemplate.gameObject.SetActive(false);
			RecipesGenerated.Send();
		}
	}
}
