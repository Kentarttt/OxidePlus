using UnityEngine;

namespace UltimateSurvival.AI
{
	public class AISettings : AIBehaviour
	{
		[SerializeField]
		private EntityMovement m_Movement;

		[SerializeField]
		private EntityDetection m_Detection;

		[SerializeField]
		private EntityVitals m_Vitals;

		[Header("Combat")]
		[SerializeField]
		[Clamp(0f, 500f)]
		private float m_HitDamage = 25f;

		[SerializeField]
		[Clamp(0f, 3f)]
		private float m_MaxAttackDistance = 2f;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_AttackSounds;

		[SerializeField]
		private SoundPlayer m_HitSounds;

		private EntityAnimation m_Animation;

		private AIBrain m_Brain;

		public EntityMovement Movement
		{
			get
			{
				return m_Movement;
			}
		}

		public EntityDetection Detection
		{
			get
			{
				return m_Detection;
			}
		}

		public EntityVitals Vitals
		{
			get
			{
				return m_Vitals;
			}
		}

		public EntityAnimation Animation
		{
			get
			{
				return m_Animation;
			}
		}

		public AudioSource AudioSource
		{
			get
			{
				return m_AudioSource;
			}
		}

		public void OnAnimationDamage()
		{
			EntityEventHandler component = m_Detection.LastChasedTarget.GetComponent<EntityEventHandler>();
			bool flag = Vector3.Distance(component.transform.position, base.transform.position) < m_MaxAttackDistance;
			bool flag2 = Vector3.Angle(m_Detection.LastChasedTarget.transform.position - base.transform.position, base.transform.forward) < 60f;
			if (component != null && flag && flag2)
			{
				component.ChangeHealth.Try(new HealthEventData(0f - m_HitDamage, base.Entity, base.transform.position + Vector3.up + base.transform.forward * 0.5f, component.transform.position - base.transform.position));
				m_HitSounds.PlayAtPosition(ItemSelectionMethod.RandomlyButExcludeLast, component.transform.position);
			}
		}

		public void PlayAttackSounds()
		{
			m_AttackSounds.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
		}

		private void Start()
		{
			m_Brain = GetComponent<AIBrain>();
			m_Movement.Initialize(m_Brain);
			m_Detection.Initialize(base.transform);
			m_Animation = new EntityAnimation();
			m_Animation.Initialize(m_Brain);
		}

		private void Update()
		{
			m_Movement.Update(base.transform);
			m_Detection.Update(m_Brain);
			m_Vitals.Update(m_Brain);
		}
	}
}
