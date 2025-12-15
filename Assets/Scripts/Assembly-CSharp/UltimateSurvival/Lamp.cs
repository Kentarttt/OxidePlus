using UnityEngine;

namespace UltimateSurvival
{
	public class Lamp : InteractableObject
	{
		public enum Mode
		{
			TimeOfDay_Dependent = 0,
			Manual = 5,
			Both = 10
		}

		[SerializeField]
		private Mode m_Mode;

		[SerializeField]
		private Light m_Light;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_ToggleAudio;

		public bool State
		{
			get
			{
				return (bool)m_Light && m_Light.enabled;
			}
		}

		public override void OnInteract(PlayerEventHandler player)
		{
			if (m_Light != null)
			{
				m_Light.enabled = !m_Light.enabled;
				m_ToggleAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
			}
			else
			{
				Debug.LogError("No Light component assigned to this Lamp!", this);
			}
		}

		private void Start()
		{
			if (m_Mode == Mode.Both || m_Mode == Mode.TimeOfDay_Dependent)
			{
				MonoSingleton<TimeOfDay>.Instance.State.AddChangeListener(OnChanged_TimeOfDay_State);
			}
			OnChanged_TimeOfDay_State();
		}

		private void OnChanged_TimeOfDay_State()
		{
			if (m_Light != null)
			{
				m_Light.enabled = MonoSingleton<TimeOfDay>.Instance.State.Is(ET.TimeOfDay.Night);
				m_ToggleAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
			}
			else
			{
				Debug.LogError("No Light component assigned to this Lamp!", this);
			}
		}

		private void OnDestroy()
		{
			if ((m_Mode == Mode.Both || m_Mode == Mode.TimeOfDay_Dependent) && MonoSingleton<TimeOfDay>.Instance != null)
			{
				MonoSingleton<TimeOfDay>.Instance.State.RemoveChangeListener(OnChanged_TimeOfDay_State);
			}
		}
	}
}
