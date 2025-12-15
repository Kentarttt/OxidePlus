using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	[RequireComponent(typeof(ItemContainer))]
	public class Hotbar : GUIBehaviour
	{
		[SerializeField]
		[Range(0f, 100f)]
		private int m_FirstSelected;

		[Header("Navigation")]
		[SerializeField]
		private bool m_EnableScrolling = true;

		[SerializeField]
		[HideSwitch("m_EnableScrolling", true, 20f)]
		[Clamp(0f, 10f)]
		private float m_ScrollThreeshold = 0.3f;

		[SerializeField]
		private bool m_SelectByDigits = true;

		[Header("Selection Graphics")]
		[SerializeField]
		private float m_SelectedSlotScale = 1f;

		[SerializeField]
		private Color m_FrameColor = Color.cyan;

		[SerializeField]
		private Sprite m_FrameSprite;

		private ItemContainer m_HotbarContainer;

		private List<Slot> m_HotbarSlots;

		private Slot m_SelectedSlot;

		private int m_LastIndex;

		private float m_CurScrollValue;

		private Image m_Frame;

		private void Awake()
		{
			m_HotbarContainer = GetComponent<ItemContainer>();
			m_HotbarContainer.Slot_Refreshed.AddListener(On_Slot_Refreshed);
			m_HotbarContainer.Slot_PointerUp += On_Slot_PointerUp;
		}

		private IEnumerator Start()
		{
			base.Player.DestroyEquippedItem.SetTryer(Try_DestroyEquippedItem);
			base.Player.Sleep.AddStopListener(delegate
			{
				TrySelectSlot(0);
			});
			m_HotbarSlots = m_HotbarContainer.Slots;
			m_Frame = GUIUtils.CreateImageUnder("Frame", GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
			m_Frame.sprite = m_FrameSprite;
			m_Frame.color = m_FrameColor;
			m_Frame.enabled = false;
			yield return null;
		}

		private bool Try_DestroyEquippedItem()
		{
			SavableItem savableItem = base.Player.EquippedItem.Get();
			if ((bool)savableItem && m_HotbarContainer.HasItem(savableItem))
			{
				base.Player.ChangeEquippedItem.Try(null, true);
			}
			return m_HotbarContainer.TryRemoveItem(savableItem);
		}

		private void On_Slot_PointerUp(PointerEventData data, Slot displayer)
		{
			if (displayer != m_SelectedSlot && data.pointerCurrentRaycast.gameObject == displayer.gameObject)
			{
				TrySelectSlot(m_HotbarSlots.IndexOf(displayer));
			}
		}

		private void On_Slot_Refreshed(Slot slot)
		{
			if (slot == m_SelectedSlot)
			{
				TrySelectSlot(m_HotbarSlots.IndexOf(m_SelectedSlot));
			}
		}

		private void Update()
		{
			int result;
			if (m_SelectByDigits && !base.Player.SelectBuildable.Active && Input.anyKeyDown && int.TryParse(Input.inputString, out result))
			{
				TrySelectSlot(result - 1);
			}
			if (m_EnableScrolling && !base.Player.SelectBuildable.Active && MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				float num = base.Player.ScrollValue.Get();
				m_CurScrollValue = Mathf.Clamp(m_CurScrollValue + num, 0f - m_ScrollThreeshold, m_ScrollThreeshold);
				if (Mathf.Abs(m_CurScrollValue - m_ScrollThreeshold * Mathf.Sign(num)) < Mathf.Epsilon)
				{
					m_CurScrollValue = 0f;
					m_LastIndex = (int)Mathf.Repeat(m_LastIndex + ((num >= 0f) ? 1 : (-1)), m_HotbarSlots.Count);
					TrySelectSlot(m_LastIndex);
				}
			}
		}

		public void TrySelectSlot(int index)
		{
			index = Mathf.Clamp(index, 0, m_HotbarSlots.Count - 1);
			Slot slot = m_HotbarSlots[index];
			base.Player.ChangeEquippedItem.Try(slot.CurrentItem, false);
			if ((bool)m_SelectedSlot)
			{
				m_SelectedSlot.SetScale(Vector3.one, 0.2f);
			}
			m_SelectedSlot = slot;
			m_SelectedSlot.SetScale(Vector3.one * m_SelectedSlotScale, 0.1f);
			m_Frame.enabled = true;
			RectTransform component = m_Frame.GetComponent<RectTransform>();
			component.SetParent(m_SelectedSlot.transform);
			component.localPosition = Vector2.zero;
			component.sizeDelta = m_SelectedSlot.GetComponent<RectTransform>().sizeDelta;
			component.localScale = Vector3.one;
		}

		public int GetSelectedItemIndex()
		{
			return m_HotbarSlots.IndexOf(m_SelectedSlot);
		}
	}
}
