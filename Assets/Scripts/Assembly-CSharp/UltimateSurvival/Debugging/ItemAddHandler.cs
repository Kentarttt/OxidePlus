using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.Debugging
{
	public class ItemAddHandler : MonoBehaviour
	{
		[SerializeField]
		private Dropdown m_ItemDropdown;

		[SerializeField]
		private InputField m_AmountInput;

		[SerializeField]
		private Button m_AddButton;

		private void Start()
		{
			m_AddButton.onClick.AddListener(TryAddItem);
			CreateItemOptions();
		}

		private void CreateItemOptions()
		{
			List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			List<string> allItemNames = database.GetAllItemNames();
			foreach (string item in allItemNames)
			{
				list.Add(new Dropdown.OptionData(item));
			}
			m_ItemDropdown.options = list;
			m_ItemDropdown.RefreshShownValue();
		}

		private void TryAddItem()
		{
			int result;
			if (int.TryParse(m_AmountInput.text, out result))
			{
				int added;
				MonoSingleton<InventoryController>.Instance.AddItemToCollection(m_ItemDropdown.value, result, "Inventory", out added);
			}
		}
	}
}
