using UnityEngine;

namespace UltimateSurvival
{
	public class EntityDeathHandler : EntityBehaviour
	{
		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_DeathAudio;

		[Header("Stuff To Disable On Death")]
		[SerializeField]
		private GameObject[] m_ObjectsToDisable;

		[SerializeField]
		private Behaviour[] m_BehavioursToDisable;

		[SerializeField]
		private Collider[] m_CollidersToDisable;

		[Header("Ragdoll")]
		[SerializeField]
		[Tooltip("On death, you can either have a ragdoll, or an animation to play.")]
		private bool m_EnableRagdoll;

		[SerializeField]
		[Tooltip("A Ragdoll component, usually attached to the armature of the character.")]
		private Ragdoll m_Ragdoll;

		[Header("Death Animation")]
		[SerializeField]
		[Tooltip("On death, you can either have a ragdoll, or an animation to play.")]
		private bool m_EnableDeathAnim;

		[SerializeField]
		private Animator m_Animator;

		[Header("Destroy Timer")]
		[SerializeField]
		[Clamp(0f, 1000f)]
		[Tooltip("")]
		private float m_DestroyTimer;

		private Vector3 m_CamStartPos;

		private Quaternion m_CamStartRot;

		private void Awake()
		{
			if (m_EnableRagdoll && !m_Ragdoll)
			{
				Debug.LogError("The ragdoll option has been enabled but no ragdoll object is assigned!", this);
			}
			base.Entity.Health.AddChangeListener(OnChanged_Health);
		}

		private void OnChanged_Health()
		{
			if (base.Entity.Health.Is(0f))
			{
				On_Death();
			}
		}

		private void On_Death()
		{
			m_DeathAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			if (m_EnableRagdoll && (bool)m_Ragdoll)
			{
				m_Ragdoll.Enable();
			}
			if (m_EnableDeathAnim && (bool)m_Animator)
			{
				m_Animator.SetTrigger("Die");
			}
			GameObject[] objectsToDisable = m_ObjectsToDisable;
			foreach (GameObject gameObject in objectsToDisable)
			{
				gameObject.SetActive(false);
			}
			Behaviour[] behavioursToDisable = m_BehavioursToDisable;
			foreach (Behaviour behaviour in behavioursToDisable)
			{
				behaviour.enabled = false;
			}
			Collider[] collidersToDisable = m_CollidersToDisable;
			foreach (Collider collider in collidersToDisable)
			{
				collider.enabled = false;
			}
			Object.Destroy(base.gameObject, m_DestroyTimer);
			base.Entity.Death.Send();
		}
	}
}
