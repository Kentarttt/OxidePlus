namespace UltimateSurvival
{
	public class AIBehaviour : EntityBehaviour
	{
		private AIEventHandler m_AI;

		public AIEventHandler AI
		{
			get
			{
				if (!m_AI)
				{
					m_AI = GetComponentInParent<AIEventHandler>();
				}
				return m_AI;
			}
		}
	}
}
