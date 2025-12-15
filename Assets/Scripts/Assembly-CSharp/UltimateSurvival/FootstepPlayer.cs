using UnityEngine;

namespace UltimateSurvival
{
	public class FootstepPlayer : PlayerBehaviour
	{
		[Header("General")]
		[SerializeField]
		private ItemSelectionMethod m_FootstepSelectionMethod;

		[SerializeField]
		private float m_LandSpeedThreeshold = 3f;

		[SerializeField]
		private LayerMask m_Mask;

		[SerializeField]
		private AudioSource m_AudioSource;

		[Header("Distance Between Steps")]
		[SerializeField]
		private float m_WalkStepLength = 1.7f;

		[SerializeField]
		private float m_RunStepLength = 2f;

		[Header("Volume Factors")]
		[SerializeField]
		private float m_WalkVolumeFactor = 0.5f;

		[SerializeField]
		private float m_RunVolumeFactor = 1f;

		private AudioSource m_LeftFootSource;

		private AudioSource m_RightFootSource;

		private FootType m_LastFroundedFoot;

		private float m_AccumulatedDistance;

		private void Start()
		{
			base.Player.Jump.AddStartListener(OnStart_PlayerJump);
			base.Player.Land.AddListener(On_PlayerLanded);
			m_LeftFootSource = GameController.Audio.CreateAudioSource("Left Foot Footstep", base.transform, new Vector3(-0.2f, 0f, 0f), false, 1f, 3f);
			m_RightFootSource = GameController.Audio.CreateAudioSource("Right Foot Footstep", base.transform, new Vector3(0.2f, 0f, 0f), false, 1f, 3f);
		}

		private void FixedUpdate()
		{
			if (base.Player.IsGrounded.Get())
			{
				m_AccumulatedDistance += base.Player.Velocity.Get().magnitude * Time.fixedDeltaTime;
				float stepLength = GetStepLength();
				if (m_AccumulatedDistance > stepLength)
				{
					PlayFootstep();
					m_AccumulatedDistance = 0f;
				}
			}
		}

		private void PlayFootstep()
		{
			SurfaceData dataFromBelow = GetDataFromBelow();
			if (dataFromBelow != null)
			{
				AudioSource audioSource = null;
				audioSource = ((m_LastFroundedFoot != 0) ? m_LeftFootSource : m_RightFootSource);
				m_LastFroundedFoot = ((m_LastFroundedFoot == FootType.Left) ? FootType.Right : FootType.Left);
				float volumeFactor = GetVolumeFactor();
				dataFromBelow.PlaySound(m_FootstepSelectionMethod, SoundType.Footstep, volumeFactor, audioSource);
			}
		}

		private float GetStepLength()
		{
			float result = m_WalkStepLength;
			if (base.Player.Run.Active)
			{
				result = m_RunStepLength;
			}
			return result;
		}

		private float GetVolumeFactor()
		{
			float result = m_WalkVolumeFactor;
			if (base.Player.Run.Active)
			{
				result = m_RunVolumeFactor;
			}
			return result;
		}

		private void OnStart_PlayerJump()
		{
			SurfaceData dataFromBelow = GetDataFromBelow();
			if (dataFromBelow != null)
			{
				dataFromBelow.PlaySound(ItemSelectionMethod.RandomlyButExcludeLast, SoundType.Jump, 1f, m_AudioSource);
			}
		}

		private void On_PlayerLanded(float landSpeed)
		{
			SurfaceData dataFromBelow = GetDataFromBelow();
			if (dataFromBelow != null && landSpeed >= m_LandSpeedThreeshold)
			{
				dataFromBelow.PlaySound(ItemSelectionMethod.RandomlyButExcludeLast, SoundType.Land, 1f, m_AudioSource);
				m_AccumulatedDistance = 0f;
			}
		}

		private SurfaceData GetDataFromBelow()
		{
			if (!GameController.SurfaceDatabase)
			{
				Debug.LogWarning("No surface database found! can't play any footsteps...", this);
				return null;
			}
			Ray ray = new Ray(base.transform.position + Vector3.up * 0.1f, Vector3.down);
			return GameController.SurfaceDatabase.GetSurfaceData(ray, 1f, m_Mask);
		}
	}
}
