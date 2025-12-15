using Mirror;
using UnityEngine;

namespace UltimateSurvival
{
	public class EntityBehaviour : MonoBehaviour
	{
		private EntityEventHandler m_Entity;

		public EntityEventHandler Entity
		{
			get
			{
				if (!m_Entity)
				{
					m_Entity = GetComponent<EntityEventHandler>();
				}
				if (!m_Entity)
				{
					m_Entity = GetComponentInParent<EntityEventHandler>();
				}
				return m_Entity;
			}
		}
	}
}
