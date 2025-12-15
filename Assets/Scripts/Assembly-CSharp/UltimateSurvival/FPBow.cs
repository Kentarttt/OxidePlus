using UnityEngine;

namespace UltimateSurvival
{
	public class FPBow : FPWeaponBase
	{
		[Header("Bow Setup")]
		[SerializeField]
		private LayerMask m_Mask;

		[SerializeField]
		private float m_MaxDistance = 50f;

		[Header("Bow Settings")]
		[SerializeField]
		private float m_MinTimeBetweenShots = 1f;

		[Header("Arrow")]
		[SerializeField]
		private ShaftedProjectile m_ArrowPrefab;

		[SerializeField]
		private Vector3 m_SpawnOffset;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_ReleaseAudio;

		[SerializeField]
		private SoundPlayer m_StretchAudio;

		private float m_NextTimeCanShoot;

		public override bool TryAttackOnce(Camera camera)
		{
			if (!base.Player.Aim.Active || Time.time < m_NextTimeCanShoot)
			{
				return false;
			}
			m_ReleaseAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			SpawnArrow(camera);
			m_NextTimeCanShoot = Time.time + m_MinTimeBetweenShots;
			base.Player.Aim.ForceStop();
			base.Attack.Send();
			return true;
		}

		protected void Awake()
		{
			base.Player.Aim.AddStartTryer(OnTryStart_Aim);
		}

		private bool OnTryStart_Aim()
		{
			bool flag = Time.time > m_NextTimeCanShoot || !base.IsEnabled;
			if (flag && base.IsEnabled)
			{
				m_StretchAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			}
			return flag;
		}

		private void SpawnArrow(Camera camera)
		{
			if (!m_ArrowPrefab)
			{
				Debug.LogErrorFormat("[{0}.FPBow] - No arrow prefab assigned in the inspector! Please assign one.", base.name);
				return;
			}
			Ray ray = camera.ViewportPointToRay(Vector3.one * 0.5f);
			RaycastHit hitInfo;
			Vector3 vector = ((!Physics.Raycast(ray, out hitInfo, m_MaxDistance, m_Mask, QueryTriggerInteraction.Ignore)) ? (camera.transform.position + camera.transform.forward * m_MaxDistance) : hitInfo.point);
			Vector3 vector2 = base.transform.position + camera.transform.TransformVector(m_SpawnOffset);
			Quaternion rotation = Quaternion.LookRotation(vector - vector2);
			GameObject gameObject = Object.Instantiate(m_ArrowPrefab.gameObject, vector2, rotation);
			gameObject.GetComponent<ShaftedProjectile>().Launch(base.Player);
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
		}
	}
}
