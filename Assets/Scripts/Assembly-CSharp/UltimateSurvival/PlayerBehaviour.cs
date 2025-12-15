namespace UltimateSurvival
{
	public class PlayerBehaviour : EntityBehaviour
	{
		private PlayerEventHandler m_Player;

		public PlayerEventHandler Player
		{
			get
			{
				if (!m_Player)
				{
					m_Player = GetComponent<PlayerEventHandler>();
				}
				if (!m_Player)
				{
					m_Player = GetComponentInParent<PlayerEventHandler>();
				}
				return m_Player;
			}
		}
	}
}
