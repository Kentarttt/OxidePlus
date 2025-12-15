using System.Collections;
using NavMeshExtension;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
	public GameObject pointer;

	private static GameObject pointerObj;

	private NavMeshAgent agent;

	private Vector3[] path;

	private void Start()
	{
		if (!pointerObj)
		{
			pointerObj = Object.Instantiate(pointer, base.transform.position, Quaternion.identity);
		}
		agent = GetComponent<NavMeshAgent>();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo))
			{
				pointerObj.transform.position = hitInfo.point;
				path = PortalManager.GetPath(base.transform.position, pointerObj.transform.position);
				StopAllCoroutines();
				StartCoroutine(GoToDestination());
			}
		}
	}

	private IEnumerator GoToDestination()
	{
		int j;
		for (j = 0; j < path.Length; j++)
		{
			agent.Warp(path[j]);
			j++;
			agent.SetDestination(path[j]);
			while (agent.pathPending)
			{
				yield return null;
			}
			float remain = agent.remainingDistance;
			while (remain == float.PositiveInfinity || remain - agent.stoppingDistance > float.Epsilon || agent.pathStatus != 0)
			{
				remain = agent.remainingDistance;
				yield return null;
			}
		}
		agent.isStopped = true;
	}
}
