using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateSurvival.GUISystem
{
	public sealed class ItemContainer : GUIBehaviour
	{
		public Message<Slot> Slot_Refreshed = new Message<Slot>();

		[SerializeField]
		private string _Name = string.Empty;

		[SerializeField]
		[Tooltip("It is optional. If you assign a window, the open state will be taken from the window, otherwise the container will always be considered open.")]
		private Window m_Window;

		[Header("Slots")]
		[SerializeField]
		[Tooltip("All the created slots will be based on this template.")]
		private Slot m_SlotTemplate;

		[SerializeField]
		[Tooltip("The parent of the slots, usually it has attached a GridLayoutGroup, HorizontalLayoutGroup, etc, so they are automatically arranged.")]
		private Transform m_SlotsParent;

		[SerializeField]
		[Range(0f, 100f)]
		private int m_PreviewSize;

		[Header("Required Stuff")]
		[SerializeField]
		[Reorderable]
		private ReorderableStringList m_RequiredCategories;

		[SerializeField]
		[Reorderable]
		private ReorderableStringList m_RequiredProperties;

		private List<ItemHolder> m_ItemHolders;

		private List<Slot> m_Slots;

		private bool m_SetUp;

		public bool IsOpen
		{
			get
			{
				return m_SetUp && (!m_Window || m_Window.IsOpen);
			}
		}

		public string Name
		{
			get
			{
				return _Name;
			}
		}

		public List<Slot> Slots { get; private set; }

		public event Action<PointerEventData, Slot> Slot_PointerDown;

		public event Action<PointerEventData, Slot> Slot_PointerUp;

		public event Action<BaseEventData, Slot> Slot_Select;

		public event DragAction Slot_BeginDrag;

		public event DragAction Slot_Drag;

		public event DragAction Slot_EndDrag;

		public void Setup(ItemHolder itemholder)
		{
			Setup(new List<ItemHolder> { itemholder });
		}

		public void Setup(List<ItemHolder> itemHolders)
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("You can't create the container when the application is not playing.", this);
			}
			else if (m_SlotTemplate == null)
			{
				Debug.LogError("You tried to create slots for this container, but the slot template is null / not assigned in the inspector!", this);
			}
			else if ((bool)m_SlotsParent && (bool)m_SlotTemplate)
			{
				if (Slots == null)
				{
					Slots = new List<Slot>();
					GetComponentsInChildren(Slots);
				}
				m_ItemHolders = itemHolders;
				bool activeSelf = m_SlotTemplate.gameObject.activeSelf;
				m_SlotTemplate.gameObject.SetActive(true);
				PrepareGUIForSlots(m_SlotsParent, m_SlotTemplate);
				m_SlotTemplate.gameObject.SetActive(activeSelf);
				m_SetUp = true;
			}
		}

		public bool HasItem(SavableItem item)
		{
			for (int i = 0; i < m_ItemHolders.Count; i++)
			{
				if ((bool)item == (bool)m_ItemHolders[i])
				{
					return true;
				}
			}
			return false;
		}

		public bool TryAddItem(ItemData itemData, int amount, out int added)
		{
			added = 0;
			CollectionUtils.AddItem(itemData, amount, Slots, out added);
			return added > 0;
		}

		public bool TryAddItem(ItemData itemData, int amount)
		{
			int added = 0;
			CollectionUtils.AddItem(itemData, amount, Slots, out added);
			return added > 0;
		}

		public bool TryAddItem(string name, int amount, out int added)
		{
			added = 0;
			ItemData itemData;
			if (MonoSingleton<InventoryController>.Instance.Database.FindItemByName(name, out itemData))
			{
				CollectionUtils.AddItem(itemData, amount, Slots, out added);
			}
			return added > 0;
		}

		public bool TryAddItem(string name, int amount)
		{
			int added = 0;
			ItemData itemData;
			if (MonoSingleton<InventoryController>.Instance.Database.FindItemByName(name, out itemData))
			{
				CollectionUtils.AddItem(itemData, amount, Slots, out added);
			}
			return added > 0;
		}

		public bool TryAddItem(SavableItem item)
		{
			if (item == null)
			{
				return false;
			}
			int added = 0;
			CollectionUtils.AddItem(item.ItemData, item.CurrentInStack, Slots, out added, item.CurrentPropertyValues);
			return added > 0;
		}

		public bool TryAddItem(ItemData itemData, int amount, int slotId, out int added)
		{
			added = 0;
			CollectionUtils.AddItem(itemData, amount, Slots[slotId], out added);
			return added > 0;
		}

		public bool TryRemoveItem(SavableItem item)
		{
			Slot slot2 = Slots.Find((Slot slot) => slot.CurrentItem == item);
			if ((bool)slot2)
			{
				slot2.ItemHolder.SetItem(null);
				return true;
			}
			return false;
		}

		public void RemoveItems(string itemName, int amount, out int removed)
		{
			CollectionUtils.RemoveItems(itemName, amount, Slots, out removed);
		}

		public void RemoveItems(string itemName, int amount)
		{
			int removed;
			CollectionUtils.RemoveItems(itemName, amount, Slots, out removed);
		}

		public void AddAllFrom(ItemContainer container)
		{
			for (int i = 0; i < container.Slots.Count; i++)
			{
				if (container.Slots[i].HasItem && TryAddItem(container.Slots[i].CurrentItem))
				{
					container.Slots[i].ItemHolder.SetItem(null);
				}
			}
		}

		public int GetItemCount(int itemID)
		{
			int num = 0;
			for (int i = 0; i < Slots.Count; i++)
			{
				if (Slots[i].HasItem && Slots[i].CurrentItem.Id == itemID)
				{
					num += Slots[i].CurrentItem.CurrentInStack;
				}
			}
			return num;
		}

		public int GetItemCount(string itemName)
		{
			int num = 0;
			for (int i = 0; i < Slots.Count; i++)
			{
				if (Slots[i].HasItem && Slots[i].CurrentItem.ItemData.Name == itemName)
				{
					num += Slots[i].CurrentItem.CurrentInStack;
				}
			}
			return num;
		}

		public void ApplyAll()
		{
			if (!Application.isPlaying)
			{
				ApplyTemplate();
				ApplyRequiredStuff();
			}
		}

		public void ApplyTemplate()
		{
			if (!Application.isPlaying)
			{
				Transform slotsParent = m_SlotsParent;
				Slot slotTemplate = m_SlotTemplate;
				if ((bool)slotsParent && (bool)slotTemplate)
				{
					bool activeSelf = slotTemplate.gameObject.activeSelf;
					slotTemplate.gameObject.SetActive(true);
					RemoveSlots(slotsParent, slotTemplate);
					CreateSlots(slotsParent, slotTemplate);
					slotTemplate.gameObject.SetActive(activeSelf);
				}
			}
		}

		public void ApplyRequiredStuff()
		{
			if (!Application.isPlaying)
			{
				Slot[] componentsInChildren = GetComponentsInChildren<Slot>();
				foreach (Slot slot in componentsInChildren)
				{
					slot.RequiredCategories = m_RequiredCategories;
					slot.RequiredProperties = m_RequiredProperties;
				}
			}
		}

		private void Awake()
		{
			Slots = new List<Slot>();
			m_SlotsParent.GetComponentsInChildren(Slots);
		}

		private void On_Slot_PointerDown(PointerEventData data, Slot slot)
		{
			if (this.Slot_PointerDown != null)
			{
				this.Slot_PointerDown(data, slot);
			}
			if (!Input.GetKey(KeyCode.LeftShift) || data.button != 0)
			{
				return;
			}
			if (_Name == "Inventory")
			{
				ItemContainer container = base.Controller.GetContainer("Loot");
				if (container.IsOpen)
				{
					if (container.TryAddItem(slot.CurrentItem))
					{
						slot.ItemHolder.SetItem(null);
					}
					return;
				}
				ItemContainer container2 = base.Controller.GetContainer("Hotbar");
				if (container2.TryAddItem(slot.CurrentItem))
				{
					slot.ItemHolder.SetItem(null);
				}
			}
			else
			{
				ItemContainer container3 = base.Controller.GetContainer("Inventory");
				if (container3.TryAddItem(slot.CurrentItem))
				{
					slot.ItemHolder.SetItem(null);
				}
			}
		}

		private void On_Slot_PointerUp(PointerEventData data, Slot slot)
		{
			if (this.Slot_PointerUp != null)
			{
				this.Slot_PointerUp(data, slot);
			}
		}

		private void On_Slot_Select(BaseEventData data, Slot slot)
		{
			if (this.Slot_Select != null)
			{
				this.Slot_Select(data, slot);
			}
		}

		private void On_Slot_BeginDrag(PointerEventData data, Slot slot)
		{
			if (this.Slot_BeginDrag != null)
			{
				this.Slot_BeginDrag(data, slot, this);
			}
		}

		private void On_Slot_Drag(PointerEventData data, Slot slot)
		{
			if (this.Slot_Drag != null)
			{
				this.Slot_Drag(data, slot, this);
			}
		}

		private void On_Slot_EndDrag(PointerEventData data, Slot slot)
		{
			if (this.Slot_EndDrag != null)
			{
				this.Slot_EndDrag(data, slot, this);
			}
		}

		private void On_Slot_Refreshed(Slot slot)
		{
			Slot_Refreshed.Send(slot);
		}

		private void ActivateSlots(Transform parent, Slot template, int count, bool active)
		{
			for (int i = 0; i < count; i++)
			{
				int index = parent.childCount - i - 1;
				if (parent.GetChild(index) != template.transform)
				{
					parent.GetChild(index).gameObject.SetActive(active);
				}
			}
		}

		private void PrepareGUIForSlots(Transform parent, Slot template)
		{
			OnSlotsDiscarded();
			ActivateSlots(parent, template, Mathf.Clamp(m_ItemHolders.Count, 0, Slots.Count), true);
			if (m_ItemHolders.Count > Slots.Count)
			{
				int num = m_ItemHolders.Count - Slots.Count;
				for (int i = 0; i < num; i++)
				{
					Slot slot = UnityEngine.Object.Instantiate(template);
					slot.transform.SetParent(parent);
					slot.transform.localPosition = Vector3.zero;
					slot.transform.localScale = Vector3.one;
					Slots.Add(slot);
				}
			}
			else if (m_ItemHolders.Count < Slots.Count)
			{
				ActivateSlots(parent, template, Slots.Count - m_ItemHolders.Count, false);
			}
			for (int j = 0; j < m_ItemHolders.Count; j++)
			{
				Slots[j].LinkWithHolder(m_ItemHolders[j]);
			}
			OnSlotsCreated();
		}

		private void OnSlotsDiscarded()
		{
			foreach (Slot slot in Slots)
			{
				slot.PointerDown -= On_Slot_PointerDown;
				slot.PointerUp -= On_Slot_PointerUp;
				slot.E_Select -= On_Slot_Select;
				slot.BeginDrag -= On_Slot_BeginDrag;
				slot.Drag -= On_Slot_Drag;
				slot.EndDrag -= On_Slot_EndDrag;
				slot.Refreshed.RemoveListener(On_Slot_Refreshed);
			}
		}

		private void OnSlotsCreated()
		{
			foreach (Slot slot in Slots)
			{
				slot.PointerDown += On_Slot_PointerDown;
				slot.PointerUp += On_Slot_PointerUp;
				slot.E_Select += On_Slot_Select;
				slot.BeginDrag += On_Slot_BeginDrag;
				slot.Drag += On_Slot_Drag;
				slot.EndDrag += On_Slot_EndDrag;
				slot.Refreshed.AddListener(On_Slot_Refreshed);
			}
		}

		private void RemoveSlots(Transform parent, Slot template)
		{
			int childCount = parent.childCount;
			for (int i = 0; i < childCount; i++)
			{
				if (parent.GetChild(parent.childCount - 1) != template.transform)
				{
					UnityEngine.Object.DestroyImmediate(parent.GetChild(parent.childCount - 1).gameObject);
				}
			}
		}

		private void CreateSlots(Transform parent, Slot template)
		{
			for (int i = 0; i < m_PreviewSize; i++)
			{
				Slot slot = UnityEngine.Object.Instantiate(template);
				slot.transform.SetParent(parent);
				slot.transform.localPosition = Vector3.zero;
				slot.transform.localScale = Vector3.one;
			}
		}
	}
}
