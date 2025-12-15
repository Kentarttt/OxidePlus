using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class BuyWindow : MonoBehaviour
	{
		public PurchasesManager purchasesManager;

		public Text description;

		public GameObject message;

		private PurchaseSlot inspectedSlot;

		public void Ask(PurchaseSlot slot)
		{
			description.text = "Do you really want to buy \"" + slot.m_ItemName + "\"?";
			inspectedSlot = slot;
			message.SetActive(true);
		}

		public void Buy()
		{
			purchasesManager.BuyItem(inspectedSlot);
			inspectedSlot = null;
			message.SetActive(false);
		}

		public void Hide()
		{
			description.text = "-";
			inspectedSlot = null;
			message.SetActive(false);
		}
	}
}
