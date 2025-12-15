using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class PurchasesManager : MonoBehaviour
	{
		public Window window;

		public MoneyManager moneyManager;

		public PurchasesList m_PurchasesList;

		public PurchasesMenu m_PurchasesMenu;

		public BuyWindow m_BuyWindow;

		public MessageManager message;

		public PlayerSave playerSave;

		public GameObject craftingButton;

		public GameObject inventoryButton;

		public GameObject controlButtons;

		public MouseLook mouseLook;

		public Button closeButton;

		private bool active;

		private void Awake()
		{
			m_PurchasesList.RecipesGenerated.AddListener(On_ItemsGenerated);
		}

		private void On_ItemsGenerated()
		{
			foreach (ShopObjectHolder item in m_PurchasesList.ItemsByCategory)
			{
				foreach (GameObject @object in item.ObjectList)
				{
					PurchaseSlot component = @object.GetComponent<PurchaseSlot>();
					if ((bool)component)
					{
						component.PointerUp += On_ItemSelected;
					}
				}
			}
		}

		private void On_ItemSelected(BaseEventData data, PurchaseSlot slot)
		{
			m_BuyWindow.Ask(slot);
		}

		public void BuyItem(PurchaseSlot slot)
		{
			ItemData result = slot.Result;
			if (moneyManager.CheckMoney(result.Purchase.Price))
			{
				int num = result.Purchase.Amount;
				if (num < 1)
				{
					num = 1;
				}
				int added;
				MonoSingleton<InventoryController>.Instance.AddItemToCollection(result.Id, num, "Inventory", out added);
				if (added > 0)
				{
					moneyManager.SpendMoney(result.Purchase.Price);
					playerSave.SavePlayer();
					MonoSingleton<MessageDisplayer>.Instance.PushMessage(string.Format("Gathered <color={0}>{1}</color> x {2}", ColorUtils.ColorToHex(Color.yellow), result.Name, added), Color.white);
				}
				else
				{
					message.ShowMessage("ERROR", "Inventory is full!");
				}
			}
		}

		public void Open()
		{
			if (!active)
			{
				active = true;
				MonoSingleton<InventoryController>.Instance.SetState.Try(ET.InventoryState.Closed);
				window.Open();
				mouseLook.lockState = true;
				controlButtons.SetActive(false);
				closeButton.onClick.AddListener(delegate
				{
					Close();
				});
				closeButton.gameObject.SetActive(true);
				inventoryButton.SetActive(true);
			}
		}

		public void Close()
		{
			if (active)
			{
				window.Close();
				mouseLook.lockState = false;
				closeButton.onClick.RemoveAllListeners();
				closeButton.gameObject.SetActive(false);
				controlButtons.SetActive(true);
				craftingButton.SetActive(false);
				inventoryButton.SetActive(false);
				active = false;
			}
		}
	}
}
