using UltimateSurvival.Building;
using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class CrosshairManager : GUIBehaviour
	{
		[Header("Messages")]
		[SerializeField]
		private GameObject m_OpenMessage;

		[SerializeField]
		private MessageForPlayer m_GrabMessage;

		[Header("Crosshairs")]
		[SerializeField]
		private CrosshairData m_DefaultCrosshair;

		[SerializeField]
		private CrosshairData[] m_CustomCrosshairs;

		private CrosshairData m_CurrentCrosshair;

		private float m_OpenGraphicHideTime;

		private void Start()
		{
			base.Player.EquippedItem.AddChangeListener(OnChanged_EquippedItem);
			base.Player.Aim.AddStartListener(OnStart_Aim);
			base.Player.Aim.AddStopListener(OnStop_Aim);
			base.Player.RaycastData.AddChangeListener(OnChanged_RaycastObject);
			MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnChanged_InventoryState);
			if ((bool)base.Player.EquippedItem.Get())
			{
				OnChanged_EquippedItem();
			}
		}

		private void Update()
		{
			if ((bool)m_CurrentCrosshair)
			{
				m_CurrentCrosshair.Update(base.Player);
			}
		}

		private void OnChanged_InventoryState()
		{
			bool isClosed = MonoSingleton<InventoryController>.Instance.IsClosed;
			if ((bool)m_CurrentCrosshair)
			{
				m_CurrentCrosshair.SetActive(isClosed);
			}
			if (!isClosed)
			{
				m_OpenMessage.SetActive(false);
				m_GrabMessage.Toggle(false);
			}
		}

		private void OnChanged_EquippedItem()
		{
			if (!MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				return;
			}
			if ((bool)m_CurrentCrosshair)
			{
				m_CurrentCrosshair.SetActive(false);
				m_CurrentCrosshair = null;
			}
			SavableItem savableItem = base.Player.EquippedItem.Get();
			if (savableItem != null)
			{
				for (int i = 0; i < m_CustomCrosshairs.Length; i++)
				{
					if (m_CustomCrosshairs[i].ItemName == savableItem.ItemData.Name)
					{
						m_CurrentCrosshair = m_CustomCrosshairs[i];
						m_CurrentCrosshair.SetActive(true);
						return;
					}
				}
			}
			m_CurrentCrosshair = m_DefaultCrosshair;
			m_CurrentCrosshair.SetActive(true);
		}

		private void OnStart_Aim()
		{
			if (m_CurrentCrosshair != null && m_CurrentCrosshair.HideWhenAiming)
			{
				m_CurrentCrosshair.SetActive(false);
			}
		}

		private void OnStop_Aim()
		{
			if (m_CurrentCrosshair != null && MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				m_CurrentCrosshair.SetActive(true);
			}
		}

		private void OnChanged_RaycastObject()
		{
			if (!MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				m_OpenMessage.SetActive(false);
				m_GrabMessage.Toggle(false);
				if ((bool)m_CurrentCrosshair)
				{
					m_CurrentCrosshair.SetActive(false);
				}
				return;
			}
			IInventoryTrigger inventoryTrigger = null;
			ItemPickup itemPickup = null;
			Door door = null;
			SleepingBag sleepingBag = null;
			Lamp lamp = null;
			RaycastData raycastData = base.Player.RaycastData.Get();
			if ((bool)raycastData && raycastData.ObjectIsInteractable)
			{
				inventoryTrigger = raycastData.GameObject.GetComponent<IInventoryTrigger>();
				itemPickup = raycastData.GameObject.GetComponent<ItemPickup>();
				door = raycastData.GameObject.GetComponent<Door>();
				sleepingBag = raycastData.GameObject.GetComponent<SleepingBag>();
				lamp = raycastData.GameObject.GetComponent<Lamp>();
			}
			m_OpenMessage.SetActive(inventoryTrigger != null);
			if (itemPickup != null && itemPickup.ItemToAdd != null)
			{
				m_GrabMessage.Toggle(true);
				m_GrabMessage.SetText(itemPickup.ItemToAdd.Name + ((itemPickup.ItemToAdd.CurrentInStack <= 1) ? string.Empty : (" x " + itemPickup.ItemToAdd.CurrentInStack)));
			}
			else if (door != null)
			{
				m_GrabMessage.Toggle(true);
				m_GrabMessage.SetText((!door.Open) ? "Open the door" : "Close the door");
			}
			else if (sleepingBag != null)
			{
				m_GrabMessage.Toggle(true);
				m_GrabMessage.SetText((MonoSingleton<TimeOfDay>.Instance.State.Get() != ET.TimeOfDay.Night) ? "You can only sleep at night time!" : "Sleep...");
			}
			else if (lamp != null)
			{
				m_GrabMessage.Toggle(true);
				m_GrabMessage.SetText(string.Format("Turn {0}", lamp.State ? "<color=red>OFF</color>" : "<color=yellow>ON</color>"));
			}
			else
			{
				m_GrabMessage.Toggle(false);
			}
			if ((bool)m_CurrentCrosshair)
			{
				bool active = !base.Player.Aim.Active && inventoryTrigger == null && itemPickup == null && door == null && sleepingBag == null;
				m_CurrentCrosshair.SetActive(active);
			}
		}
	}
}
