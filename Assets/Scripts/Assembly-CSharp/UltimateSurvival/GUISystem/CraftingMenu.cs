using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class CraftingMenu : MonoBehaviour
	{
		public Value<CraftingCategory> SelectedCategory = new Value<CraftingCategory>(null);

		[SerializeField]
		private CraftingCategory m_FirstSelected;

		[SerializeField]
		private GameObject m_SelectionHighlight;

		[SerializeField]
		private Text m_CategoryName;

		private CraftingCategory[] m_Categories;

		private GameObject m_SpawnedBackground;

		private void Awake()
		{
			m_Categories = GetComponentsInChildren<CraftingCategory>();
			if (m_Categories.Length > 0)
			{
				CraftingCategory[] categories = m_Categories;
				foreach (CraftingCategory craftingCategory in categories)
				{
					craftingCategory.Selected.AddListener(On_CategorySelected);
				}
			}
			else
			{
				Debug.LogWarning("No categories were found as children, this menu is useless!", this);
			}
		}

		private void Start()
		{
			if ((bool)m_SelectionHighlight)
			{
				m_SpawnedBackground = Object.Instantiate(m_SelectionHighlight, m_Categories[0].transform.parent);
				m_SpawnedBackground.transform.localScale = Vector3.one;
				m_SpawnedBackground.SetActive(false);
			}
			if ((bool)m_FirstSelected)
			{
				SelectedCategory.Set(m_FirstSelected);
				On_CategorySelected(m_FirstSelected);
			}
		}

		private void On_CategorySelected(CraftingCategory selectedCategory)
		{
			if ((bool)m_SelectionHighlight)
			{
				if (!m_SpawnedBackground.activeSelf)
				{
					m_SpawnedBackground.SetActive(true);
				}
				if (m_SpawnedBackground.transform.GetSiblingIndex() > 0)
				{
					m_SpawnedBackground.transform.SetAsFirstSibling();
				}
				m_SpawnedBackground.transform.position = selectedCategory.transform.position;
			}
			if ((bool)m_CategoryName)
			{
				m_CategoryName.text = selectedCategory.DisplayName;
			}
			SelectedCategory.Set(selectedCategory);
		}
	}
}
