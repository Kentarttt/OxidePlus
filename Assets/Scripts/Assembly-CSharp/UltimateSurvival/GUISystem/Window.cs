using System.Collections;
using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	[RequireComponent(typeof(Animator))]
	public class Window : MonoBehaviour
	{
		public enum OpenTrigger
		{
			InventoryOpened = 0,
			SpecificState = 1,
			Manual = 2
		}

		[Header("Animation Speed")]
		[SerializeField]
		private float m_HideSpeed = 1.3f;

		[SerializeField]
		private float m_ShowSpeed = 1.3f;

		[SerializeField]
		private float m_RefreshSpeed = 1.3f;

		[Header("How Is Opened")]
		[SerializeField]
		[Tooltip("Inventory Opened - will open when the inventory is opened. \nSpecific State - will open when the inventory opens in a specific state (e.g. Furnace-mode, Campfire-mode). \nManual - will have to be manually opened from another script.")]
		private OpenTrigger m_OpenTrigger;

		[SerializeField]
		private ET.InventoryState m_StateTrigger = ET.InventoryState.Normal;

		private Animator m_Animator;

		private GameObject content;

		public bool IsOpen { get; private set; }

		public virtual void Open()
		{
			if (IsOpen)
			{
				m_Animator.SetTrigger("Refresh");
				return;
			}
			StopAllCoroutines();
			GetComponent<CanvasGroup>().interactable = true;
			GetComponent<CanvasGroup>().blocksRaycasts = true;
			UpdateAnimatorParams();
			m_Animator.SetTrigger("Show");
			content.SetActive(true);
			IsOpen = true;
		}

		public virtual void Close(bool instant = false)
		{
			if (IsOpen)
			{
				UpdateAnimatorParams();
				if (!instant)
				{
					m_Animator.SetTrigger("Hide");
					StartCoroutine(C_DisableWithDelay());
				}
				else
				{
					m_Animator.Play("Hide", 0, 1f);
					Disable();
				}
				IsOpen = false;
			}
		}

		public void Refresh()
		{
			m_Animator.SetTrigger("Refresh");
		}

		protected virtual void Start()
		{
			IsOpen = true;
			content = base.transform.GetChild(0).gameObject;
			content.SetActive(false);
			m_Animator = GetComponent<Animator>();
			UpdateAnimatorParams();
			MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnChanged_InventoryState);
			Close(true);
		}

		private void OnChanged_InventoryState()
		{
			bool flag = false;
			if (!MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				flag = m_OpenTrigger != OpenTrigger.Manual && (m_OpenTrigger == OpenTrigger.InventoryOpened || MonoSingleton<InventoryController>.Instance.State.Is(m_StateTrigger));
			}
			if (flag)
			{
				Open();
			}
			else
			{
				Close();
			}
		}

		private IEnumerator C_DisableWithDelay()
		{
			yield return new WaitForSeconds(0.5f);
			Disable();
		}

		private void Disable()
		{
			content.SetActive(false);
			GetComponent<CanvasGroup>().interactable = false;
			GetComponent<CanvasGroup>().blocksRaycasts = false;
		}

		private void OnValidate()
		{
			UpdateAnimatorParams();
		}

		private void UpdateAnimatorParams()
		{
			if ((bool)m_Animator)
			{
				m_Animator.SetFloat("Hide Speed", m_HideSpeed);
				m_Animator.SetFloat("Show Speed", m_ShowSpeed);
				m_Animator.SetFloat("Refresh Speed", m_RefreshSpeed);
			}
		}
	}
}
