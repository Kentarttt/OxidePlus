using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class CharacterEquipmentHandler : MonoBehaviour
	{
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_EquipAudio;

		private void Start()
		{
			Slot[] componentsInChildren = GetComponentsInChildren<Slot>();
			foreach (Slot slot in componentsInChildren)
			{
				slot.ItemHolder.Updated.AddListener(On_ItemHolder_Updated);
			}
		}

		private void On_ItemHolder_Updated(ItemHolder holder)
		{
			MonoSingleton<InventoryController>.Instance.EquipmentChanged.Send(holder);
			if (holder.HasItem)
			{
				m_EquipAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
			}
		}
	}
}
