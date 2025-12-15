using UnityEngine;

namespace UltimateSurvival
{
	public class PlayerVitals : EntityVitals
	{
		[Header("Stamina")]
		[SerializeField]
		[Clamp(0f, 300f)]
		private float m_StaminaDepletionRate = 30f;

		[SerializeField]
		private StatRegenData m_StaminaRegeneration;

		[SerializeField]
		private SoundPlayer m_BreathingHeavyAudio;

		[SerializeField]
		private float m_BreathingHeavyDuration = 11f;

		[SerializeField]
		[Clamp(0f, 100f)]
		private float m_JumpStaminaTake = 15f;

		[Header("Thirst")]
		[SerializeField]
		private StatDepleter m_ThirstDepletion;

		[Header("Hunger")]
		[SerializeField]
		private StatDepleter m_HungerDepletion;

		[Header("Sleeping")]
		[SerializeField]
		private bool m_SleepRestoresHealth = true;

		private PlayerEventHandler m_Player;

		private float m_LastHeavyBreathTime;

		public PlayerEventHandler Player
		{
			get
			{
				if (!m_Player)
				{
					m_Player = GetComponent<PlayerEventHandler>();
				}
				if (!m_Player)
				{
					m_Player = GetComponentInParent<PlayerEventHandler>();
				}
				return m_Player;
			}
		}

		protected override void Update()
		{
			base.Update();
			if (Player.Run.Active)
			{
				m_StaminaRegeneration.Pause();
				ModifyStamina((0f - m_StaminaDepletionRate) * Time.deltaTime);
			}
			else if (m_StaminaRegeneration.CanRegenerate && Player.Stamina.Get() < 100f)
			{
				ModifyStamina(m_StaminaRegeneration.RegenDelta);
			}
			if (!m_StaminaRegeneration.CanRegenerate && Player.Stamina.Is(0f) && Time.time - m_LastHeavyBreathTime > m_BreathingHeavyDuration)
			{
				m_LastHeavyBreathTime = Time.time;
				m_BreathingHeavyAudio.Play2D();
			}
			m_ThirstDepletion.Update(Player.Thirst, Player.ChangeHealth);
			m_HungerDepletion.Update(Player.Hunger, Player.ChangeHealth);
		}

		protected override bool Try_ChangeHealth(HealthEventData healthEventData)
		{
			healthEventData.Delta *= 1f - (float)Player.Defense.Get() / 100f;
			return base.Try_ChangeHealth(healthEventData);
		}

		private void Start()
		{
			Player.Run.AddStartTryer(delegate
			{
				m_StaminaRegeneration.Pause();
				return Player.Stamina.Get() > 0f;
			});
			Player.Jump.AddStartListener(delegate
			{
				ModifyStamina(0f - m_JumpStaminaTake);
			});
			if (m_SleepRestoresHealth)
			{
				Player.Sleep.AddStopListener(delegate
				{
					Player.ChangeHealth.Try(new HealthEventData(100f));
				});
			}
		}

		private void ModifyStamina(float delta)
		{
			float value = Player.Stamina.Get() + delta;
			value = Mathf.Clamp(value, 0f, 100f);
			Player.Stamina.Set(value);
		}
	}
}
