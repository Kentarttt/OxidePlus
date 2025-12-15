using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class SurfaceData
	{
		private const int BULLET_DECAL_POOL_SIZE = 100;

		private const int BULLET_IMPACT_FX_POOL_SIZE = 100;

		private const int CHOP_FX_POOL_SIZE = 25;

		private const int HIT_FX_POOL_SIZE = 25;

		[SerializeField]
		private string m_Name;

		[SerializeField]
		private Texture[] m_Textures;

		[Header("Footsteps")]
		[SerializeField]
		private SoundPlayer m_FootstepSounds;

		[SerializeField]
		private SoundPlayer m_JumpSounds;

		[SerializeField]
		private SoundPlayer m_LandSounds;

		[Header("Bullet Impact")]
		[SerializeField]
		private SoundPlayer m_BulletImpactSounds;

		[SerializeField]
		private GameObject[] m_BulletDecals;

		[SerializeField]
		private GameObject[] m_BulletImpactFX;

		[Header("Chop & Hit")]
		[SerializeField]
		private SoundPlayer m_ChopSounds;

		[SerializeField]
		private SoundPlayer m_HitSounds;

		[SerializeField]
		private GameObject[] m_ChopFX;

		[SerializeField]
		private GameObject[] m_HitFX;

		[Header("Penetration")]
		[SerializeField]
		private bool m_IsPenetrable;

		[SerializeField]
		private SoundPlayer m_SpearPenetrationSounds;

		[SerializeField]
		private SoundPlayer m_ArrowPenetrationSounds;

		private int m_LastPlayedFootstep;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public bool IsPenetrable
		{
			get
			{
				return m_IsPenetrable;
			}
		}

		public bool HasTexture(Texture texture)
		{
			for (int i = 0; i < m_Textures.Length; i++)
			{
				if (m_Textures[i] == texture)
				{
					return true;
				}
			}
			return false;
		}

		public void PlaySound(ItemSelectionMethod selectionMethod, SoundType soundType, float volumeFactor = 1f, AudioSource audioSource = null)
		{
			switch (soundType)
			{
			case SoundType.BulletImpact:
				m_BulletImpactSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.Footstep:
				m_FootstepSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.Jump:
				m_JumpSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.Land:
				m_LandSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.Chop:
				m_ChopSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.Hit:
				m_HitSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.SpearPenetration:
				m_SpearPenetrationSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			case SoundType.ArrowPenetration:
				m_ArrowPenetrationSounds.Play(selectionMethod, audioSource, volumeFactor);
				break;
			}
		}

		public void PlaySound(ItemSelectionMethod selectionMethod, SoundType soundType, float volumeFactor = 1f, Vector3 position = default(Vector3))
		{
			switch (soundType)
			{
			case SoundType.BulletImpact:
				m_BulletImpactSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.Footstep:
				m_FootstepSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.Jump:
				m_JumpSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.Land:
				m_LandSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.Chop:
				m_ChopSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.Hit:
				m_HitSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.SpearPenetration:
				m_SpearPenetrationSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			case SoundType.ArrowPenetration:
				m_ArrowPenetrationSounds.PlayAtPosition(selectionMethod, position, volumeFactor);
				break;
			}
		}

		public void CreateBulletDecal(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (m_BulletDecals.Length != 0)
			{
				GameObject gameObject = m_BulletDecals[UnityEngine.Random.Range(0, m_BulletDecals.Length)];
				if ((bool)gameObject)
				{
					UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
					return;
				}
				Debug.LogWarningFormat("[({0}) SurfaceData] - A decal object was found null, please check the surface database for missing decals.", Name);
			}
		}

		public void CreateBulletImpactFX(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (m_BulletImpactFX.Length != 0)
			{
				GameObject gameObject = m_BulletImpactFX[UnityEngine.Random.Range(0, m_BulletImpactFX.Length)];
				if ((bool)gameObject)
				{
					UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
					return;
				}
				Debug.LogWarningFormat("[({0}) SurfaceData] - A bullet impact FX prefab was found null, please check the surface database for missing effects.", Name);
			}
		}

		public void CreateHitFX(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (m_HitFX.Length != 0)
			{
				GameObject gameObject = m_HitFX[UnityEngine.Random.Range(0, m_HitFX.Length)];
				if ((bool)gameObject)
				{
					UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
					return;
				}
				Debug.LogWarningFormat("[({0}) SurfaceData] - A hit FX prefab was found null, please check the surface database for missing effects.", Name);
			}
		}

		public void CreateChopFX(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (m_ChopFX.Length != 0)
			{
				GameObject gameObject = m_ChopFX[UnityEngine.Random.Range(0, m_ChopFX.Length)];
				if ((bool)gameObject)
				{
					UnityEngine.Object.Instantiate(gameObject, position, rotation, parent);
					return;
				}
				Debug.LogWarningFormat("[({0}) SurfaceData] - A chop FX prefab was found null, please check the surface database for missing effects.", Name);
			}
		}
	}
}
