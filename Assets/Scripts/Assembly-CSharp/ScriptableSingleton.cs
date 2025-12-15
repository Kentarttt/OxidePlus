using UnityEngine;

public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
{
	private static T m_Instance;

	public static T Instance
	{
		get
		{
			if (!m_Instance)
			{
				T[] array = Resources.LoadAll<T>(string.Empty);
				if (array.Length > 0)
				{
					m_Instance = array[0];
				}
				else
				{
					Debug.LogErrorFormat("[ScriptableSingleton] - No object of type '{0}' was found in the project!", typeof(T).Name);
				}
			}
			return m_Instance;
		}
	}
}
