using UnityEngine;
using UnityEngine.AI;

namespace NavMeshExtension
{
	public class AgentAnimator : MonoBehaviour
	{
		private NavMeshAgent nAgent;

		private Animator animator;

		private void Start()
		{
			animator = GetComponentInChildren<Animator>();
			nAgent = GetComponent<NavMeshAgent>();
		}

		private void OnAnimatorMove()
		{
			float num = 0f;
			float num2 = 0f;
			num = nAgent.velocity.magnitude;
			Vector3 vector = Quaternion.Inverse(base.transform.rotation) * nAgent.desiredVelocity;
			num2 = Mathf.Atan2(vector.x, vector.z) * 180f / 3.14159f;
			animator.SetFloat("Speed", num);
			animator.SetFloat("Direction", num2, 0.15f, Time.deltaTime);
		}
	}
}
