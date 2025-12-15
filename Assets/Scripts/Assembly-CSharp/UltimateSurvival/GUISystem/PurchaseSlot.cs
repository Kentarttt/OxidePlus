using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class PurchaseSlot : Selectable
	{
		public delegate void BaseAction(BaseEventData data, PurchaseSlot slot);

		public delegate void PointerAction(PointerEventData data, PurchaseSlot slot);

		[Header("Setup")]
		[SerializeField]
		private Text m_PurchaseName;

		[SerializeField]
		private Image m_IconHolder;

		[SerializeField]
		private Text m_PriceHolder;

		public string m_ItemName;

		public int m_ItemPrice;

		public int m_ItemAmount;

		public ItemData Result { get; private set; }

		public event PointerAction PointerUp;

		public void ShowPurchaseItem(ItemData item)
		{
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			Result = item;
			m_ItemName = ((!(item.DisplayName == string.Empty)) ? item.DisplayName : item.Name) + ((item.Purchase.Amount <= 1) ? string.Empty : (" <color=#ffffffff>x" + item.Purchase.Amount + "</color>"));
			m_ItemPrice = item.Purchase.Price;
			m_ItemAmount = item.Purchase.Amount;
			m_PurchaseName.text = m_ItemName;
			m_IconHolder.sprite = item.Icon;
			m_PriceHolder.text = m_ItemPrice.ToString();
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
