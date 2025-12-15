using UnityEngine;

namespace UltimateSurvival
{
	public class FPMelee : FPWeaponBase
	{
		public enum HitInvokeMethod
		{
			ByTimer = 0,
			ByAnimationEvent = 1
		}

		[Header("Melee Setup")]
		[SerializeField]
		[Tooltip("The animation event handler - that picks up animation events from Mecanim.")]
		private FPMeleeEventHandler m_EventHandler;

		[Header("Melee Settings")]
		[SerializeField]
		[Tooltip("From how far can this object hit stuff?")]
		private float m_MaxReach = 0.5f;

		[SerializeField]
		[Tooltip("Useful for limiting the number of hits you can do in a period of time.")]
		private float m_TimeBetweenAttacks = 0.85f;

		[SerializeField]
		[Range(0f, 1000f)]
		private float m_DamagePerHit = 15f;

		[Range(0f, 1000f)]
		[SerializeField]
		private float m_ImpactForce = 15f;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_AttackAudio;

		[SerializeField]
		private SoundType m_SoundType;

		private Message<bool> m_MeleeAttack = new Message<bool>();

		private Message m_Miss = new Message();

		private Message m_Hit = new Message();

		private float m_NextUseTime;

		public Message<bool> MeleeAttack
		{
			get
			{
				return m_MeleeAttack;
			}
		}

		public Message Miss
		{
			get
			{
				return m_Miss;
			}
		}

		public Message Hit
		{
			get
			{
				return m_Hit;
			}
		}

		public float MaxReach
		{
			get
			{
				return m_MaxReach;
			}
		}

		public float DamagePerHit
		{
			get
			{
				return m_DamagePerHit;
			}
		}

		public override bool TryAttackOnce(Camera camera)
		{
			if (Time.time < m_NextUseTime)
			{
				return false;
			}
			RaycastData raycastData = base.Player.RaycastData.Get();
			if ((bool)raycastData)
			{
				bool message = raycastData.HitInfo.distance < m_MaxReach;
				MeleeAttack.Send(message);
			}
			else
			{
				MeleeAttack.Send(false);
			}
			base.Attack.Send();
			m_NextUseTime = Time.time + m_TimeBetweenAttacks;
			return true;
		}

		public override bool TryAttackContinuously(Camera camera)
		{
			return TryAttackOnce(camera);
		}

		protected virtual void Start()
		{
			m_EventHandler.Hit.AddListener(On_Hit);
			m_EventHandler.Woosh.AddListener(On_Woosh);
		}

		protected virtual void On_Hit()
		{
			RaycastData raycastData = base.Player.RaycastData.Get();
			if (!raycastData)
			{
				return;
			}
			if ((bool)GameController.SurfaceDatabase)
			{
				SurfaceData surfaceData = GameController.SurfaceDatabase.GetSurfaceData(raycastData.HitInfo);
				surfaceData.PlaySound(ItemSelectionMethod.Randomly, m_SoundType, 1f, raycastData.HitInfo.point);
				if (m_SoundType == SoundType.Hit)
				{
					surfaceData.CreateHitFX(raycastData.HitInfo.point, Quaternion.LookRotation(raycastData.HitInfo.normal));
				}
				else if (m_SoundType == SoundType.Chop)
				{
					surfaceData.CreateChopFX(raycastData.HitInfo.point, Quaternion.LookRotation(raycastData.HitInfo.normal));
				}
			}
			IDamageable component = raycastData.GameObject.GetComponent<IDamageable>();
			if (component != null)
			{
				HealthEventData damageData = new HealthEventData(0f - m_DamagePerHit, base.Player, base.transform.position, GameController.WorldCamera.transform.forward, m_ImpactForce);
				component.ReceiveDamage(damageData);
			}
			if (m_Durability != null)
			{
				ItemProperty.Float @float = m_Durability.Float;
				@float.Current -= 1f;
				m_Durability.SetValue(ItemProperty.Type.Float, @float);
				if (@float.Current == 0f)
				{
					base.Player.DestroyEquippedItem.Try();
				}
			}
			Hit.Send();
		}

		private void On_Woosh()
		{
			m_AttackAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource, 0.6f);
			Miss.Send();
		}
	}
}
