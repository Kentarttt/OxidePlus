using UnityEngine;

public class CannibalResetter : MonoBehaviour
{
	[SerializeField]
	private GameObject m_CannibalPrefab;

	[SerializeField]
	private Transform[] m_SpawnPoints;

	[SerializeField]
	private Transform m_Container;

	private void Start()
	{
		ResetCannibals();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.O))
		{
			ResetCannibals();
		}
	}

	private void ResetCannibals()
	{
		int childCount = m_Container.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy(m_Container.GetChild(i).gameObject);
		}
		for (int j = 0; j < m_SpawnPoints.Length; j++)
		{
			Object.Instantiate(m_CannibalPrefab, m_SpawnPoints[j].position, m_SpawnPoints[j].rotation, m_Container);
		}
	}
}
