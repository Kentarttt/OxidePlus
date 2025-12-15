using UnityEngine;
using Mirror;

namespace UltimateSurvival
{
	public class ReloadLoop : StateMachineBehaviour
	{
		private float m_NextReloadTime;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			m_NextReloadTime = Time.time + stateInfo.length;
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateUpdate(animator, stateInfo, layerIndex);
			if (Time.time >= m_NextReloadTime)
			{
				int integer = animator.GetInteger("Reload Loop Count");
				if (integer > 0)
				{
					m_NextReloadTime = Time.time + stateInfo.length;
					animator.SetInteger("Reload Loop Count", integer - 1);
				}
			}
		}
	}
}
