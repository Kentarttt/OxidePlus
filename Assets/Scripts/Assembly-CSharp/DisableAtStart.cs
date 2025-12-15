using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
	[SerializeField]
	private GameObject[] m_Objects;

	private void Start()
	{
		GameObject[] objects = m_Objects;
		foreach (GameObject gameObject in objects)
		{
			gameObject.SetActive(false);
		}
	}
}
