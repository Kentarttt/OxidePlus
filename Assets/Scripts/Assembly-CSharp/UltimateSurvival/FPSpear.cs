using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class FPSpear : FPWeaponBase
	{
		[Header("Setup")]
		[SerializeField]
		private LayerMask m_Mask;

		[SerializeField]
		private float m_MaxDistance = 50f;

		[Header("Settings")]
		[SerializeField]
		private float m_MinTimeBetweenThrows = 1.5f;

		[Header("Object To Throw")]
		[SerializeField]
		private ShaftedProjectile m_SpearPrefab;

		[SerializeField]
		private Vector3 m_SpawnOffset;

		[SerializeField]
		private float m_SpawnDelay = 0.3f;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_ThrowAudio;

		private float m_NextTimeCanThrow;

		public bool CanThrow
		{
			get
			{
				return Time.time > m_NextTimeCanThrow;
			}
		}

		public override bool TryAttackOnce(Camera camera)
		{
			if (!base.Player.Aim.Active || !CanThrow)
			{
				return false;
			}
			m_ThrowAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			StartCoroutine(C_ThrowWithDelay(camera, m_SpawnDelay));
			m_NextTimeCanThrow = Time.time + m_MinTimeBetweenThrows;
			base.Attack.Send();
			return true;
		}

		private void Start()
		{
			base.Player.Aim.AddStartTryer(OnTryStart_Aim);
		}

		private bool OnTryStart_Aim()
		{
			return !base.IsEnabled || CanThrow;
		}

		private IEnumerator C_ThrowWithDelay(Camera camera, float delay)
		{
			if (!m_SpearPrefab)
			{
				Debug.LogErrorFormat(this, "The spear prefab is not assigned in the inspector!.", base.name);
				yield break;
			}
			yield return new WaitForSeconds(delay);
			Ray ray = camera.ViewportPointToRay(Vector3.one * 0.5f);
			RaycastHit hitInfo;
			Vector3 hitPoint = ((!Physics.Raycast(ray, out hitInfo, m_MaxDistance, m_Mask, QueryTriggerInteraction.Ignore)) ? (camera.transform.position + camera.transform.forward * m_MaxDistance) : hitInfo.point);
			Vector3 position = base.transform.position + camera.transform.TransformVector(m_SpawnOffset);
			GameObject spearObject = Object.Instantiate(rotation: Quaternion.LookRotation(hitPoint - position), original: m_SpearPrefab.gameObject, position: position);
			spearObject.GetComponent<ShaftedProjectile>().Launch(base.Player);
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
