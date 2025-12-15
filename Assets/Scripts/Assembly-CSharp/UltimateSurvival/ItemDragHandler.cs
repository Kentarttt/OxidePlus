using System.Collections.Generic;
using UltimateSurvival.GUISystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateSurvival
{
	public class ItemDragHandler : MonoSingleton<ItemDragHandler>
	{
		[SerializeField]
		private float m_DraggedItemScale = 0.85f;

		[SerializeField]
		private float m_DraggedItemAlpha = 0.75f;

		[SerializeField]
		private KeyCode m_SplitKey = KeyCode.LeftShift;

		private ItemContainer[] m_AllCollections;

		private bool m_Dragging;

		private SavableItem m_DraggedItem;

		private RectTransform m_DraggedItemRT;

		private Canvas m_Canvas;

		private RectTransform m_ParentCanvasRT;

		private Vector2 m_DragOffset;

		public bool IsDragging
		{
			get
			{
				return m_Dragging;
			}
		}

		public event DropAction PlayerDroppedItem;

		private void Start()
		{
			m_AllCollections = MonoSingleton<GUIController>.Instance.Containers;
			m_Canvas = MonoSingleton<GUIController>.Instance.Canvas;
			m_ParentCanvasRT = m_Canvas.GetComponent<RectTransform>();
			ItemContainer[] allCollections = m_AllCollections;
			foreach (ItemContainer itemContainer in allCollections)
			{
				itemContainer.Slot_BeginDrag += On_Slot_BeginDrag;
				itemContainer.Slot_Drag += On_Slot_Drag;
				itemContainer.Slot_EndDrag += On_Slot_EndDrag;
			}
		}

		private void On_Slot_BeginDrag(PointerEventData data, Slot slot, ItemContainer collection)
		{
			if (slot.HasItem && !MonoSingleton<InventoryController>.Instance.IsClosed && Input.touchCount <= 1)
			{
				m_Dragging = true;
				SavableItem currentItem = slot.CurrentItem;
				if (Input.GetKey(m_SplitKey) && currentItem.CurrentInStack > 1)
				{
					int currentInStack = currentItem.CurrentInStack;
					int num = currentInStack / 2;
					currentItem.CurrentInStack = currentInStack - num;
					m_DraggedItem = new SavableItem(currentItem.ItemData, num, currentItem.CurrentPropertyValues);
					slot.Refresh();
				}
				else
				{
					slot.ItemHolder.SetItem(null);
					m_DraggedItem = currentItem;
				}
				m_DraggedItemRT = slot.GetDragTemplate(m_DraggedItem, m_DraggedItemAlpha);
				m_DraggedItemRT.SetParent(m_ParentCanvasRT, true);
				m_DraggedItemRT.localScale = Vector3.one * m_DraggedItemScale;
				Camera pressEventCamera = data.pressEventCamera;
				Vector3 worldPoint;
				if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_ParentCanvasRT, (Vector3)data.position, pressEventCamera, out worldPoint))
				{
					m_DragOffset = slot.transform.position - worldPoint;
				}
			}
		}

		private void On_Slot_Drag(PointerEventData data, Slot initialSlot, ItemContainer collection)
		{
			if (m_Dragging)
			{
				Camera pressEventCamera = data.pressEventCamera;
				Vector2 localPoint;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentCanvasRT, data.position, pressEventCamera, out localPoint))
				{
					m_DraggedItemRT.localPosition = localPoint + (Vector2)m_ParentCanvasRT.InverseTransformVector(m_DragOffset);
				}
			}
		}

		private void On_Slot_EndDrag(PointerEventData data, Slot initialSlot, ItemContainer collection)
		{
			if (!m_Dragging)
			{
				return;
			}
			List<Slot> slots = collection.Slots;
			GameObject gameObject = data.pointerCurrentRaycast.gameObject;
			Slot slot = null;
			if ((bool)gameObject)
			{
				slot = gameObject.GetComponent<Slot>();
			}
			if ((bool)slot)
			{
				if (slot.AllowsItem(m_DraggedItem))
				{
					if (!slot.HasItem)
					{
						if ((bool)slot.ItemHolder)
						{
							slot.ItemHolder.SetItem(m_DraggedItem);
						}
						else
						{
							Debug.LogError("You tried to drop an item over a Slot which is not linked with any holder.", this);
						}
					}
					else
					{
						SavableItem currentItem = slot.CurrentItem;
						if (currentItem.Id == m_DraggedItem.Id && currentItem.ItemData.StackSize > 1 && currentItem.CurrentInStack < currentItem.ItemData.StackSize)
						{
							OnItemsAreStackable(slot, initialSlot);
						}
						else
						{
							On_ItemNotStackable(slot, initialSlot);
						}
					}
				}
				else
				{
					PutItemBack(initialSlot);
				}
				initialSlot.Refresh();
			}
			else
			{
				if (this.PlayerDroppedItem != null)
				{
					this.PlayerDroppedItem(collection, initialSlot, m_DraggedItem);
				}
				if (!MonoSingleton<InventoryController>.Instance.Try_DropItem(m_DraggedItem))
				{
					collection.TryAddItem(m_DraggedItem);
				}
			}
			Object.Destroy(m_DraggedItemRT.gameObject);
			m_DraggedItem = null;
			m_Dragging = false;
		}

		private void OnItemsAreStackable(Slot slotUnderPointer, Slot initialSlot)
		{
			int added;
			slotUnderPointer.ItemHolder.TryAddItem(m_DraggedItem.ItemData, m_DraggedItem.CurrentInStack, out added, m_DraggedItem.CurrentPropertyValues);
			int num = m_DraggedItem.CurrentInStack - added;
			if (num > 0)
			{
				if (initialSlot.HasItem)
				{
					slotUnderPointer.Parent.TryAddItem(m_DraggedItem.ItemData, num);
				}
				else
				{
					initialSlot.ItemHolder.SetItem(new SavableItem(m_DraggedItem.ItemData, num, m_DraggedItem.CurrentPropertyValues));
				}
			}
		}

		private void On_ItemNotStackable(Slot slotUnderPointer, Slot initialSlot)
		{
			if (!initialSlot.AllowsItem(slotUnderPointer.CurrentItem))
			{
				PutItemBack(initialSlot);
				return;
			}
			SavableItem currentItem = slotUnderPointer.CurrentItem;
			if (!initialSlot.HasItem)
			{
				slotUnderPointer.ItemHolder.SetItem(m_DraggedItem);
				initialSlot.ItemHolder.SetItem(currentItem);
				return;
			}
			int added;
			initialSlot.Parent.TryAddItem(m_DraggedItem.ItemData, m_DraggedItem.CurrentInStack, out added);
			int num = m_DraggedItem.CurrentInStack - added;
			if (num > 0)
			{
				initialSlot.Parent.TryAddItem(m_DraggedItem.ItemData, num);
			}
		}

		private void PutItemBack(Slot initialSlot)
		{
			if (initialSlot.HasItem)
			{
				initialSlot.Parent.TryAddItem(m_DraggedItem);
			}
			else
			{
				initialSlot.ItemHolder.SetItem(m_DraggedItem);
			}
		}
	}
}
