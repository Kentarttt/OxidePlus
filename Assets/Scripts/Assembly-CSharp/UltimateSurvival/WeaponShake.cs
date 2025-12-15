using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class WeaponShake
	{
		[Header("Shake")]
		[SerializeField]
		private float m_Magnitude = 15f;

		[SerializeField]
		private float m_Roughness = 3f;

		[SerializeField]
		private Vector3 m_PositionInfluence = new Vector3(0.01f, 0.01f, 0.01f);

		[SerializeField]
		private Vector3 m_RotationInfluence = new Vector3(0.8f, 0.5f, 0.5f);

		[SerializeField]
		private float m_FadeInTime = 0.2f;

		[SerializeField]
		private float m_FadeOutTime = 0.3f;

		public void Shake()
		{
			MonoSingleton<FPCameraController>.Instance.Shake(GetShakeInstance());
		}

		private ShakeInstance GetShakeInstance()
		{
			ShakeInstance shakeInstance = new ShakeInstance(m_Magnitude, m_Roughness, m_FadeInTime, m_FadeOutTime);
			shakeInstance.PositionInfluence = m_PositionInfluence;
			shakeInstance.RotationInfluence = m_RotationInfluence;
			shakeInstance.IsWeapon = true;
			return shakeInstance;
		}
	}
}
