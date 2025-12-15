using UnityEngine;

namespace UltimateSurvival
{
	public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = Object.FindObjectOfType<T>();
				}
				return m_Instance;
			}
		}
	}
}
