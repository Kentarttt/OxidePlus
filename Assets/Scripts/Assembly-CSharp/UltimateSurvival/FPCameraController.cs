using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class FPCameraController : MonoSingleton<FPCameraController>
	{
		[Tooltip("The default position influence of all shakes created on the fly.")]
		[SerializeField]
		private Vector3 m_DefaultPosInfluence = new Vector3(0.15f, 0.15f, 0.15f);

		[Tooltip("The default rotation influence of all shakes created on the fly.")]
		[SerializeField]
		private Vector3 m_DefaultRotInfluence = new Vector3(1f, 1f, 1f);

		[Header("Headbobs")]
		[SerializeField]
		private PlayerEventHandler m_Player;

		[SerializeField]
		private TrigonometricBob m_WalkHeadbob;

		[SerializeField]
		private TrigonometricBob m_RunHeadbob;

		[Header("Shakes")]
		[SerializeField]
		private GenericShake m_DamageShake;

		[SerializeField]
		private WeaponShake m_LandShake;

		[SerializeField]
		private float m_LandThreeshold = 3f;

		private Vector3 m_PositionAddShake;

		private Vector3 m_RotationAddShake;

		private Vector3 m_PositionAddBob;

		private Vector3 m_RotationAddBob;

		private List<ShakeInstance> m_ShakeInstances = new List<ShakeInstance>();

		public ShakeInstance Shake(ShakeInstance shake)
		{
			m_ShakeInstances.Add(shake);
			return shake;
		}

		public ShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime)
		{
			ShakeInstance shakeInstance = new ShakeInstance(magnitude, roughness, fadeInTime, fadeOutTime);
			shakeInstance.PositionInfluence = m_DefaultPosInfluence;
			shakeInstance.RotationInfluence = m_DefaultRotInfluence;
			m_ShakeInstances.Add(shakeInstance);
			return shakeInstance;
		}

		public ShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 posInfluence, Vector3 rotInfluence)
		{
			ShakeInstance shakeInstance = new ShakeInstance(magnitude, roughness, fadeInTime, fadeOutTime);
			shakeInstance.PositionInfluence = posInfluence;
			shakeInstance.RotationInfluence = rotInfluence;
			m_ShakeInstances.Add(shakeInstance);
			return shakeInstance;
		}

		public ShakeInstance StartShake(float magnitude, float roughness, float fadeInTime)
		{
			ShakeInstance shakeInstance = new ShakeInstance(magnitude, roughness);
			shakeInstance.PositionInfluence = m_DefaultPosInfluence;
			shakeInstance.RotationInfluence = m_DefaultRotInfluence;
			shakeInstance.StartFadeIn(fadeInTime);
			m_ShakeInstances.Add(shakeInstance);
			return shakeInstance;
		}

		public ShakeInstance StartShake(float magnitude, float roughness, float fadeInTime, Vector3 posInfluence, Vector3 rotInfluence)
		{
			ShakeInstance shakeInstance = new ShakeInstance(magnitude, roughness);
			shakeInstance.PositionInfluence = posInfluence;
			shakeInstance.RotationInfluence = rotInfluence;
			shakeInstance.StartFadeIn(fadeInTime);
			m_ShakeInstances.Add(shakeInstance);
			return shakeInstance;
		}

		private void Awake()
		{
			m_Player.ChangeHealth.AddListener(OnSuccess_PlayerHealthChanged);
			m_Player.Land.AddListener(On_PlayerLanded);
		}

		private void OnSuccess_PlayerHealthChanged(HealthEventData healthEventData)
		{
			if (healthEventData.Delta < 0f && healthEventData.Delta < -8f)
			{
				m_DamageShake.Shake(Mathf.Abs(healthEventData.Delta / 100f));
			}
		}

		private void On_PlayerLanded(float landSpeed)
		{
			if (landSpeed > m_LandThreeshold)
			{
				m_LandShake.Shake();
			}
		}

		private void LateUpdate()
		{
			m_PositionAddShake = Vector3.zero;
			m_RotationAddShake = Vector3.zero;
			for (int i = 0; i < m_ShakeInstances.Count && i < m_ShakeInstances.Count; i++)
			{
				ShakeInstance shakeInstance = m_ShakeInstances[i];
				if (shakeInstance.CurrentState == ShakeState.Inactive && shakeInstance.DeleteOnInactive)
				{
					m_ShakeInstances.RemoveAt(i);
					i--;
				}
				else if (shakeInstance.CurrentState != ShakeState.Inactive)
				{
					m_PositionAddShake += Vector3.Scale(shakeInstance.UpdateShake(), shakeInstance.PositionInfluence);
					m_RotationAddShake += Vector3.Scale(shakeInstance.UpdateShake(), shakeInstance.RotationInfluence);
				}
			}
			float magnitude = m_Player.Velocity.Get().magnitude;
			if (m_Player.Walk.Active)
			{
				m_PositionAddBob = m_WalkHeadbob.CalculateBob(magnitude, Time.deltaTime);
			}
			else
			{
				m_PositionAddBob = m_WalkHeadbob.Cooldown(Time.deltaTime);
			}
			if (m_Player.Run.Active)
			{
				m_PositionAddBob += m_RunHeadbob.CalculateBob(magnitude, Time.deltaTime);
			}
			else
			{
				m_PositionAddBob += m_RunHeadbob.Cooldown(Time.deltaTime);
			}
			base.transform.localPosition = m_PositionAddShake + m_PositionAddBob;
			base.transform.localEulerAngles = m_RotationAddShake;
		}
	}
}
