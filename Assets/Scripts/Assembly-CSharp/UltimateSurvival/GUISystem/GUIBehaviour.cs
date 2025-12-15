using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class GUIBehaviour : MonoBehaviour
	{
		private GUIController m_Controller;

		public GUIController Controller
		{
			get
			{
				if (m_Controller == null)
				{
					m_Controller = GetComponentInParent<GUIController>();
				}
				return m_Controller;
			}
		}

		public PlayerEventHandler Player
		{
			get
			{
				return (!Controller) ? null : Controller.Player;
			}
		}
	}
}
