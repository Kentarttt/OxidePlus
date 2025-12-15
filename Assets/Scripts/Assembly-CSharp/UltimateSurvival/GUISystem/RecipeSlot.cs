using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class RecipeSlot : Selectable
	{
		public delegate void BaseAction(BaseEventData data, RecipeSlot slot);

		public delegate void PointerAction(PointerEventData data, RecipeSlot slot);

		[Header("Setup")]
		[SerializeField]
		private Text m_RecipeName;

		[SerializeField]
		private Image m_Icon;

		public ItemData Result { get; private set; }

		public event BaseAction E_Deselect;

		public event PointerAction PointerUp;

		public void ShowRecipeForItem(ItemData item)
		{
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			Result = item;
			m_RecipeName.text = ((!(item.DisplayName == string.Empty)) ? item.DisplayName : item.Name);
			m_Icon.sprite = item.Icon;
		}

		public override void OnDeselect(BaseEventData data)
		{
			if (MonoSingleton<GUIController>.Instance.MouseOverSelectionKeeper())
			{
				StartCoroutine(C_WaitAndSelect(1));
			}
			else
			{
				base.OnDeselect(data);
			}
		}

		public override void OnPointerUp(PointerEventData data)
		{
			base.OnPointerUp(data);
			if (this.PointerUp != null)
			{
				this.PointerUp(data, this);
			}
		}

		private IEnumerator C_WaitAndSelect(int waitFrameCount)
		{
			for (int i = 0; i < waitFrameCount; i++)
			{
				yield return null;
			}
			Select();
		}
	}
}
