using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class Slot : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler, IEventSystemHandler
	{
		public delegate void BaseAction(BaseEventData data, Slot wrapper);

		public delegate void PointerAction(PointerEventData data, Slot wrapper);

		public Message<Slot> Refreshed = new Message<Slot>();

		[Header("Setup")]
		[SerializeField]
		private Image m_ItemIcon;

		[SerializeField]
		private Text m_StackDisplayer;

		[SerializeField]
		private DurabilityBar m_DurabilityBar;

		[Header("Required Stuff")]
		[SerializeField]
		[Reorderable]
		private ReorderableStringList m_RequiredCategories;

		[SerializeField]
		[Reorderable]
		private ReorderableStringList m_RequiredProperties;

		private Coroutine m_ScaleSetter;

		public ItemHolder ItemHolder { get; private set; }

		public ItemContainer Parent { get; private set; }

		public bool HasItem
		{
			get
			{
				return (bool)ItemHolder && ItemHolder.HasItem;
			}
		}

		public SavableItem CurrentItem
		{
			get
			{
				return (!ItemHolder) ? null : ItemHolder.CurrentItem;
			}
		}

		public ReorderableStringList RequiredCategories
		{
			get
			{
				return m_RequiredCategories;
			}
			set
			{
				m_RequiredCategories = value;
			}
		}

		public ReorderableStringList RequiredProperties
		{
			get
			{
				return m_RequiredProperties;
			}
			set
			{
				m_RequiredProperties = value;
			}
		}

		public event BaseAction E_Select;

		public event BaseAction E_Deselect;

		public event PointerAction PointerDown;

		public event PointerAction PointerUp;

		public event PointerAction BeginDrag;

		public event PointerAction Drag;

		public event PointerAction EndDrag;

		public bool AllowsItem(SavableItem item)
		{
			bool flag = false;
			bool flag2 = true;
			if (m_RequiredProperties.Count > 0)
			{
				for (int i = 0; i < m_RequiredProperties.Count; i++)
				{
					if (!item.HasProperty(m_RequiredProperties[i]))
					{
						flag2 = false;
						break;
					}
				}
			}
			if (m_RequiredCategories.Count > 0)
			{
				for (int j = 0; j < m_RequiredCategories.Count; j++)
				{
					if (m_RequiredCategories[j] == item.ItemData.Category)
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			return flag && flag2;
		}

		public void LinkWithHolder(ItemHolder holder)
		{
			if ((bool)ItemHolder)
			{
				ItemHolder.Updated.RemoveListener(On_ItemHolder_Updated);
			}
			ItemHolder = holder;
			Refresh();
			ItemHolder.Updated.AddListener(On_ItemHolder_Updated);
		}

		public void Refresh()
		{
			m_ItemIcon.enabled = HasItem;
			m_StackDisplayer.enabled = HasItem && CurrentItem.CurrentInStack > 1;
			m_DurabilityBar.SetActive(HasItem && CurrentItem.HasProperty("Durability"));
			if (m_ItemIcon.enabled)
			{
				m_ItemIcon.sprite = CurrentItem.ItemData.Icon;
			}
			if (m_StackDisplayer.enabled)
			{
				m_StackDisplayer.text = string.Format("x{0}", CurrentItem.CurrentInStack);
			}
			if (m_DurabilityBar.Active)
			{
				m_DurabilityBar.SetFillAmount(CurrentItem.GetPropertyValue("Durability").Float.Ratio);
			}
			Refreshed.Send(this);
		}

		public RectTransform GetDragTemplate(SavableItem forItem, float alpha)
		{
			Slot slot = Object.Instantiate(this);
			Transform transform = slot.transform.FindDeepChild("Frame");
			if (transform != null)
			{
				Object.Destroy(transform.gameObject);
			}
			slot.enabled = false;
			slot.image.enabled = false;
			slot.m_ItemIcon.enabled = true;
			slot.m_StackDisplayer.enabled = forItem.CurrentInStack > 1;
			slot.m_StackDisplayer.text = string.Format("x{0}", forItem.CurrentInStack);
			slot.m_DurabilityBar.SetActive(forItem.HasProperty("Durability"));
			CanvasGroup canvasGroup = slot.gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = alpha;
			return slot.GetComponent<RectTransform>();
		}

		public void SetScale(Vector3 localScale, float duration)
		{
			if (!IsDestroyed())
			{
				if (m_ScaleSetter != null)
				{
					StopCoroutine(m_ScaleSetter);
				}
				m_ScaleSetter = StartCoroutine(C_SetScale(localScale, duration));
			}
		}

		public override void OnSelect(BaseEventData data)
		{
			base.OnSelect(data);
			if (this.E_Select != null)
			{
				this.E_Select(data, this);
			}
		}

		public override void OnDeselect(BaseEventData data)
		{
			if (HasItem && !MonoSingleton<InventoryController>.Instance.IsClosed && MonoSingleton<GUIController>.Instance.MouseOverSelectionKeeper())
			{
				StartCoroutine(C_WaitAndSelect(1));
				return;
			}
			if (this.E_Deselect != null)
			{
				this.E_Deselect(data, this);
			}
			base.OnDeselect(data);
		}

		public override void OnPointerDown(PointerEventData data)
		{
			base.OnPointerDown(data);
			if (this.PointerDown != null)
			{
				this.PointerDown(data, this);
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

		public void OnBeginDrag(PointerEventData data)
		{
			if (this.BeginDrag != null)
			{
				this.BeginDrag(data, this);
			}
		}

		public void OnDrag(PointerEventData data)
		{
			if (this.Drag != null)
			{
				this.Drag(data, this);
			}
		}

		public void OnEndDrag(PointerEventData data)
		{
			if (this.EndDrag != null)
			{
				this.EndDrag(data, this);
			}
			if (data.pointerCurrentRaycast.gameObject != base.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
				DoStateTransition((!HasItem) ? SelectionState.Disabled : SelectionState.Normal, true);
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (Application.isPlaying)
			{
				Parent = GetComponentInParent<ItemContainer>();
				m_ItemIcon.enabled = false;
				m_StackDisplayer.enabled = false;
				m_DurabilityBar.SetActive(false);
			}
		}

		private void On_ItemHolder_Updated(ItemHolder holder)
		{
			Refresh();
		}

		private IEnumerator C_SetScale(Vector3 localScale, float duration)
		{
			float finishTime = Time.time + duration;
			float scaleChangeSpeed = (localScale - base.transform.localScale).magnitude / duration;
			while (Time.time < finishTime)
			{
				base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, localScale, scaleChangeSpeed * Time.deltaTime);
				yield return null;
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
