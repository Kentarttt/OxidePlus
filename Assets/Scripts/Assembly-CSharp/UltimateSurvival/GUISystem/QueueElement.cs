using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class QueueElement : MonoBehaviour
	{
		public Message<QueueElement> Cancel = new Message<QueueElement>();

		public Message Complete = new Message();

		[SerializeField]
		private Image m_Icon;

		[SerializeField]
		private Text m_RemainedTime;

		[SerializeField]
		private Image m_ProgressBar;

		private CraftData m_CraftData;

		private ItemData m_Result;

		private ItemContainer m_Inventory;

		private int m_AmountToCraftRemained;

		private bool m_Initialized;

		public void Initialize(CraftData craftData, ItemContainer inventory)
		{
			if (!m_Initialized)
			{
				RequiredItem[] requiredItems = craftData.Result.Recipe.RequiredItems;
				foreach (RequiredItem requiredItem in requiredItems)
				{
					inventory.RemoveItems(requiredItem.Name, requiredItem.Amount * craftData.Amount);
				}
				m_Result = craftData.Result;
				m_Icon.sprite = m_Result.Icon;
				m_RemainedTime.text = string.Format("{0}s (x{1})", craftData.Result.Recipe.Duration, craftData.Amount);
				m_ProgressBar.fillAmount = 0f;
				m_AmountToCraftRemained = craftData.Amount;
				m_CraftData = craftData;
				m_Inventory = inventory;
				m_Initialized = true;
			}
		}

		public void StartCrafting()
		{
			if (m_Initialized)
			{
				StartCoroutine(C_Update(m_CraftData, m_Inventory));
			}
		}

		public void CancelCrafting()
		{
			if (m_AmountToCraftRemained > 0)
			{
				RequiredItem[] requiredItems = m_CraftData.Result.Recipe.RequiredItems;
				foreach (RequiredItem requiredItem in requiredItems)
				{
					ItemData itemData;
					MonoSingleton<InventoryController>.Instance.Database.FindItemByName(requiredItem.Name, out itemData);
					m_Inventory.TryAddItem(itemData, requiredItem.Amount * m_AmountToCraftRemained);
				}
			}
			Cancel.Send(this);
			Object.Destroy(base.gameObject);
		}

		private IEnumerator C_Update(CraftData craftData, ItemContainer inventory)
		{
			while (m_AmountToCraftRemained > 0)
			{
				float remainedTime = craftData.Result.Recipe.Duration;
				while (remainedTime > 0f)
				{
					yield return null;
					remainedTime = Mathf.Clamp(remainedTime - Time.deltaTime, 0f, float.PositiveInfinity);
					m_RemainedTime.text = string.Format("{0}s (x{1})", Mathf.Ceil(remainedTime), m_AmountToCraftRemained);
					m_ProgressBar.fillAmount = 1f - remainedTime / (float)craftData.Result.Recipe.Duration;
				}
				inventory.TryAddItem(m_Result, 1);
				m_AmountToCraftRemained--;
				m_ProgressBar.fillAmount = 0f;
				m_RemainedTime.text = string.Format("{0}s (x{1})", 0, m_AmountToCraftRemained);
			}
			Complete.Send();
			Object.Destroy(base.gameObject);
		}
	}
}
