using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class FPHitscan : FPWeaponBase
	{
		public enum ShellSpawnMethod
		{
			Auto = 0,
			OnAnimationEvent = 1
		}

		[Header("Gun Setup")]
		[SerializeField]
		private ET.FireMode m_FireMode;

		[SerializeField]
		[Tooltip("The layers that will be affected when you fire.")]
		private LayerMask m_Mask;

		[SerializeField]
		[Tooltip("If something is farther than this distance threeshold, it will not be affected by the shot.")]
		private float m_MaxDistance = 150f;

		[Header("Fire Mode.Semi Auto")]
		[SerializeField]
		[Tooltip("The minimum time that can pass between consecutive shots.")]
		private float m_FireDuration = 0.22f;

		[Header("Fire Mode.Burst")]
		[SerializeField]
		[Tooltip("How many shots will the gun fire when in Burst-mode.")]
		private int m_BurstLength = 3;

		[SerializeField]
		[Tooltip("How much time it takes to fire all the shots.")]
		private float m_BurstDuration = 0.3f;

		[SerializeField]
		[Tooltip("The minimum time that can pass between consecutive bursts.")]
		private float m_BurstPause = 0.35f;

		[Header("Fire Mode.Full Auto")]
		[SerializeField]
		[Tooltip("The maximum amount of shots that can be executed in a minute.")]
		private int m_RoundsPerMinute = 450;

		[Header("Gun Settings")]
		[Range(1f, 20f)]
		[SerializeField]
		[Tooltip("The amount of rays that will be sent in the world (basically the amount of projectiles / bullets that will be fired at a time).")]
		private int m_RayCount = 1;

		[Range(0f, 30f)]
		[SerializeField]
		[Tooltip("When NOT aiming, how much the projectiles will spread (in angles).")]
		private float m_NormalSpread = 0.8f;

		[SerializeField]
		[Range(0f, 30f)]
		[Tooltip("When aiming, how much the projectiles will spread (in angles).")]
		private float m_AimSpread = 0.95f;

		[SerializeField]
		[Tooltip("")]
		private RayImpact m_RayImpact;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		[Tooltip("Sounds that will play when firing the gun.")]
		private SoundPlayer m_FireAudio;

		[Header("Effects")]
		[SerializeField]
		private ParticleSystem m_MuzzleFlash;

		[SerializeField]
		private GameObject m_Tracer;

		[SerializeField]
		private Vector3 m_TracerOffset;

		[Header("Shell")]
		[SerializeField]
		private Rigidbody m_ShellPrefab;

		[SerializeField]
		private ShellSpawnMethod m_ShellSpawnMethod;

		[SerializeField]
		private FPHitscanEventHandler m_AnimEventHandler;

		[SerializeField]
		private Transform m_WeaponRoot;

		[SerializeField]
		private Vector3 m_ShellSpawnOffset;

		[SerializeField]
		private Vector3 m_ShellSpawnVelocity = new Vector3(3f, 2f, 0.3f);

		[SerializeField]
		private float m_ShellSpin = 0.3f;

		private float m_MinTimeBetweenShots;

		private float m_NextTimeCanFire;

		private WaitForSeconds m_BurstWait;

		public override bool TryAttackOnce(Camera camera)
		{
			if (Time.time < m_NextTimeCanFire || !base.IsEnabled)
			{
				return false;
			}
			m_NextTimeCanFire = Time.time + m_MinTimeBetweenShots;
			if (m_FireMode == ET.FireMode.Burst)
			{
				StartCoroutine(C_DoBurst(camera));
			}
			else
			{
				Shoot(camera);
			}
			return true;
		}

		public override bool TryAttackContinuously(Camera camera)
		{
			if (m_FireMode == ET.FireMode.SemiAuto)
			{
				return false;
			}
			return TryAttackOnce(camera);
		}

		protected IEnumerator C_DoBurst(Camera camera)
		{
			for (int i = 0; i < m_BurstLength; i++)
			{
				Shoot(camera);
				yield return m_BurstWait;
			}
		}

		protected void Shoot(Camera camera)
		{
			m_FireAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			if ((bool)m_MuzzleFlash)
			{
				m_MuzzleFlash.Play(true);
			}
			for (int i = 0; i < m_RayCount; i++)
			{
				DoHitscan(camera);
			}
			base.Attack.Send();
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
			GameController.Audio.LastGunshot.Set(new Gunshot(base.transform.position, base.Player));
		}

		protected void DoHitscan(Camera camera)
		{
			float num = ((!base.Player.Aim.Active) ? m_NormalSpread : m_AimSpread);
			Ray ray = camera.ViewportPointToRay(Vector2.one * 0.5f);
			Vector3 euler = camera.transform.TransformVector(new Vector3(Random.Range(0f - num, num), Random.Range(0f - num, num), 0f));
			ray.direction = Quaternion.Euler(euler) * ray.direction;
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, m_MaxDistance, m_Mask, QueryTriggerInteraction.Ignore))
			{
				float impulseAtDistance = m_RayImpact.GetImpulseAtDistance(hitInfo.distance, m_MaxDistance);
				float damageAtDistance = m_RayImpact.GetDamageAtDistance(hitInfo.distance, m_MaxDistance);
				IDamageable component = hitInfo.collider.GetComponent<IDamageable>();
				if (component != null)
				{
					HealthEventData damageData = new HealthEventData(0f - damageAtDistance, base.Player, hitInfo.point, ray.direction, impulseAtDistance);
					component.ReceiveDamage(damageData);
				}
				else if ((bool)hitInfo.rigidbody)
				{
					hitInfo.rigidbody.AddForceAtPosition(ray.direction * impulseAtDistance, hitInfo.point, ForceMode.Impulse);
				}
				if ((bool)GameController.SurfaceDatabase)
				{
					SurfaceData surfaceData = GameController.SurfaceDatabase.GetSurfaceData(hitInfo);
					surfaceData.CreateBulletDecal(hitInfo.point + hitInfo.normal * 0.01f, Quaternion.LookRotation(hitInfo.normal), hitInfo.collider.transform);
					surfaceData.CreateBulletImpactFX(hitInfo.point + hitInfo.normal * 0.01f, Quaternion.LookRotation(hitInfo.normal));
					surfaceData.PlaySound(ItemSelectionMethod.Randomly, SoundType.BulletImpact, 1f, hitInfo.point);
				}
			}
			if ((bool)m_Tracer)
			{
				Object.Instantiate(m_Tracer, base.transform.position + base.transform.TransformVector(m_TracerOffset), Quaternion.LookRotation(ray.direction));
			}
			if ((bool)m_ShellPrefab && m_ShellSpawnMethod == ShellSpawnMethod.Auto)
			{
				SpawnShell();
			}
		}

		private void Start()
		{
			m_BurstWait = new WaitForSeconds(m_BurstDuration / (float)m_BurstLength);
			if (m_FireMode == ET.FireMode.SemiAuto)
			{
				m_MinTimeBetweenShots = m_FireDuration;
			}
			else if (m_FireMode == ET.FireMode.Burst)
			{
				m_MinTimeBetweenShots = m_BurstDuration + m_BurstPause;
			}
			else
			{
				m_MinTimeBetweenShots = 60f / (float)m_RoundsPerMinute;
			}
			if (m_ShellSpawnMethod == ShellSpawnMethod.OnAnimationEvent && m_AnimEventHandler != null)
			{
				m_AnimEventHandler.AnimEvent_SpawnObject.AddListener(On_AnimEvent_SpawnObject);
			}
		}

		private void OnValidate()
		{
			m_BurstWait = new WaitForSeconds(m_BurstDuration / (float)m_BurstLength);
			if (m_FireMode == ET.FireMode.SemiAuto)
			{
				m_MinTimeBetweenShots = m_FireDuration;
			}
			else if (m_FireMode == ET.FireMode.Burst)
			{
				m_MinTimeBetweenShots = m_BurstDuration + m_BurstPause;
			}
			else
			{
				m_MinTimeBetweenShots = 60f / (float)m_RoundsPerMinute;
			}
		}

		private void On_AnimEvent_SpawnObject(string name)
		{
			if (name == "Shell")
			{
				SpawnShell();
			}
		}

		private void SpawnShell()
		{
			Rigidbody rigidbody = Object.Instantiate(m_ShellPrefab, m_WeaponRoot.position + m_WeaponRoot.TransformVector(m_ShellSpawnOffset), Random.rotation);
			Rigidbody component = rigidbody.GetComponent<Rigidbody>();
			component.angularVelocity = new Vector3(Random.Range(0f - m_ShellSpin, m_ShellSpin), Random.Range(0f - m_ShellSpin, m_ShellSpin), Random.Range(0f - m_ShellSpin, m_ShellSpin));
			component.linearVelocity = base.transform.TransformVector(m_ShellSpawnVelocity);
		}
	}
}
