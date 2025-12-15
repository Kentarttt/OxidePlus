using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class ItemInspector : GUIBehaviour
	{
		[Header("Setup")]
		[SerializeField]
		private Camera m_GUICamera;

		[SerializeField]
		private Window m_Window;

		[Header("Item Info")]
		[SerializeField]
		private Text m_ItemName;

		[SerializeField]
		private Text m_MainDescription;

		[SerializeField]
		private Text m_SecondaryDescription;

		[SerializeField]
		private Image m_Icon;

		[SerializeField]
		private DurabilityBar m_DurabilityBar;

		[SerializeField]
		private Text m_Magazine;

		[Header("Actions")]
		[SerializeField]
		private Button m_DropButton;

		[SerializeField]
		private Button m_ConsumeButton;

		[SerializeField]
		private Button m_DismantleButton;

		[Header("Audio")]
		[SerializeField]
		private SoundPlayer m_ItemDropAudio;

		[SerializeField]
		private SoundPlayer m_DismantleAudio;

		[SerializeField]
		private float m_ConsumeVolume = 0.6f;

		private ItemContainer[] m_InspectableContainers;

		private ItemContainer m_InventoryContainer;

		private float m_LastConsumeTime;

		public Slot InspectedSlot { get; private set; }

		private void Awake()
		{
			m_DismantleButton.onClick.AddListener(On_DismantleClicked);
			m_DropButton.onClick.AddListener(On_DropClicked);
			m_ConsumeButton.onClick.AddListener(On_ConsumeClicked);
		}

		private void Start()
		{
			m_InspectableContainers = base.Controller.Containers;
			ItemContainer[] inspectableContainers = m_InspectableContainers;
			foreach (ItemContainer itemContainer in inspectableContainers)
			{
				itemContainer.Slot_PointerUp += On_Slot_PointerUp;
			}
			m_InventoryContainer = base.Controller.GetContainer("Inventory");
		}

		private void On_Slot_PointerUp(BaseEventData data, Slot slot)
		{
			if (!MonoSingleton<InventoryController>.Instance.IsClosed && slot.HasItem && EventSystem.current.currentSelectedGameObject == slot.gameObject)
			{
				if ((bool)m_Window)
				{
					m_Window.Open();
				}
				InspectedSlot = slot;
				ShowInfo(slot.CurrentItem);
				InspectedSlot.E_Deselect += delegate
				{
					StartCoroutine(C_WaitAndSelect());
				};
				InspectedSlot.ItemHolder.Updated.AddListener(On_InspectedHolderUpdated);
			}
			else
			{
				StopAllCoroutines();
				StartCoroutine(C_WaitAndSelect());
			}
		}

		private void ShowInfo(SavableItem item)
		{
			m_ItemName.text = ((!(item.ItemData.DisplayName == string.Empty)) ? item.ItemData.DisplayName : item.ItemData.Name);
			if (item.ItemData.Descriptions.Length > 0)
			{
				m_MainDescription.text = item.GetDescription(0);
			}
			else
			{
				m_MainDescription.text = string.Empty;
			}
			if (item.ItemData.Descriptions.Length > 1)
			{
				m_SecondaryDescription.text = item.GetDescription(1);
			}
			else
			{
				m_SecondaryDescription.text = string.Empty;
			}
			m_Icon.sprite = item.ItemData.Icon;
			if (item.HasProperty("Durability"))
			{
				if (!m_DurabilityBar.Active)
				{
					m_DurabilityBar.SetActive(true);
				}
				m_DurabilityBar.SetFillAmount(item.GetPropertyValue("Durability").Float.Ratio);
			}
			else if (m_DurabilityBar.Active)
			{
				m_DurabilityBar.SetActive(false);
			}
			ItemProperty.Value propertyValue;
			if (item.FindPropertyValue("Magazine", out propertyValue))
			{
				ItemProperty.IntRange intRange = propertyValue.IntRange;
				m_Magazine.text = "Magazine: " + intRange.ToString();
			}
			else
			{
				m_Magazine.text = string.Empty;
			}
			m_ConsumeButton.gameObject.SetActive(item.HasProperty("Can Consume"));
			m_DismantleButton.gameObject.SetActive(item.HasProperty("Can Dismantle"));
		}

		private void On_InspectedHolderUpdated(ItemHolder holder)
		{
			if (!holder.HasItem)
			{
				m_Window.Close();
				try
				{
					InspectedSlot.ItemHolder.Updated.RemoveListener(On_InspectedHolderUpdated);
					InspectedSlot = null;
				}
				catch
				{
				}
			}
		}

		private void On_DismantleClicked()
		{
			SavableItem currentItem = InspectedSlot.CurrentItem;
			InspectedSlot.ItemHolder.SetItem(null);
			RequiredItem[] requiredItems = currentItem.ItemData.Recipe.RequiredItems;
			for (int i = 0; i < requiredItems.Length; i++)
			{
				int amount = Mathf.RoundToInt((float)requiredItems[i].Amount * currentItem.GetPropertyValue("Durability").Float.Ratio * 0.6f) + 1;
				m_InventoryContainer.TryAddItem(requiredItems[i].Name, amount);
			}
			MonoSingleton<MessageDisplayer>.Instance.PushMessage(string.Format("<color=yellow>{0}</color> has been dismantled", currentItem.Name));
			m_DismantleAudio.Play2D();
		}

		private void On_DropClicked()
		{
			SavableItem savableItem = base.Controller.Player.EquippedItem.Get();
			SavableItem currentItem = InspectedSlot.CurrentItem;
			if (savableItem == currentItem)
			{
				base.Controller.Player.ChangeEquippedItem.Try(null, true);
			}
			if (MonoSingleton<InventoryController>.Instance.Try_DropItem(InspectedSlot.CurrentItem, InspectedSlot))
			{
				m_ItemDropAudio.Play2D();
				EventSystem.current.SetSelectedGameObject(null);
			}
		}

		private void On_ConsumeClicked()
		{
			SavableItem currentItem = InspectedSlot.CurrentItem;
			if (currentItem.HasProperty("Can Consume") && Time.time - m_LastConsumeTime > 3f)
			{
				if (currentItem.HasProperty("Thirst Change") && (base.Player.Thirst.Get() < 100f || base.Player.Thirst.Get() > 0f))
				{
					base.Player.Thirst.Set(base.Player.Thirst.Get() + (float)currentItem.GetPropertyValue("Thirst Change").RandomInt.RandomValue);
				}
				if (currentItem.HasProperty("Hunger Change") && (base.Player.Hunger.Get() < 100f || base.Player.Hunger.Get() > 0f))
				{
					base.Player.Hunger.Set(base.Player.Hunger.Get() + (float)currentItem.GetPropertyValue("Hunger Change").RandomInt.RandomValue);
				}
				if (currentItem.HasProperty("Health Change"))
				{
					base.Player.ChangeHealth.Try(new HealthEventData(currentItem.GetPropertyValue("Health Change").RandomInt.RandomValue));
				}
				if (currentItem.HasProperty("Consume Sound"))
				{
					GameController.Audio.Play2D(currentItem.GetPropertyValue("Consume Sound").Sound, m_ConsumeVolume);
				}
				if (currentItem.CurrentInStack == 1)
				{
					InspectedSlot.ItemHolder.SetItem(null);
					EventSystem.current.SetSelectedGameObject(null);
				}
				else
				{
					currentItem.CurrentInStack--;
				}
				m_LastConsumeTime = Time.time;
			}
		}

		private IEnumerator C_WaitAndSelect()
		{
			yield return null;
			GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
			Slot selectedSlot = null;
			if ((bool)currentSelected)
			{
				selectedSlot = currentSelected.GetComponent<Slot>();
			}
			if (!currentSelected || !selectedSlot || !selectedSlot.HasItem)
			{
				if ((bool)m_Window)
				{
					m_Window.Close();
				}
				InspectedSlot = null;
			}
		}
	}
}
