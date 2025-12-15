using Mirror;
using UltimateSurvival.StandardAssets;
using UnityEngine;

namespace UltimateSurvival
{
	public class PlayerPauseHandler : MonoBehaviour
	{
		[SerializeField]
		private DOF m_DOF;

		[SerializeField]
		private ColorCorrection m_ColorCorrectionCurves;

		[SerializeField]
		private GameObject m_Background;

		private void Start()
		{
			MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnInventoryToggled);
		}

		private void OnInventoryToggled()
		{
			bool isClosed = MonoSingleton<InventoryController>.Instance.IsClosed;
			if ((bool)m_DOF)
			{
				m_DOF.enabled = !isClosed;
			}
			if ((bool)m_ColorCorrectionCurves)
			{
				m_ColorCorrectionCurves.enabled = !isClosed;
			}
			if ((bool)m_Background)
			{
				m_Background.SetActive(!isClosed);
			}
		}

		public void SetActive(bool active)
		{
			if ((bool)m_DOF)
			{
				m_DOF.enabled = active;
			}
			if ((bool)m_Background)
			{
				m_Background.SetActive(active);
			}
		}
	}
}
