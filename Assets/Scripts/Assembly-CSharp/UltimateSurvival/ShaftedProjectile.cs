using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ShaftedProjectile : MonoBehaviour
	{
		[Header("Setup")]
		[SerializeField]
		private Transform m_Model;

		[SerializeField]
		private Transform m_Pivot;

		[SerializeField]
		private LayerMask m_Mask;

		[SerializeField]
		private float m_MaxDistance = 2f;

		[Header("Launch")]
		[SerializeField]
		private float m_LaunchSpeed = 50f;

		[Header("Damage")]
		[SerializeField]
		private float m_MaxDamage = 100f;

		[SerializeField]
		[Tooltip("How the damage changes, when the speed gets lower.")]
		private AnimationCurve m_DamageCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.8f, 0.5f), new Keyframe(1f, 0f));

		[Header("Penetration")]
		[SerializeField]
		[Tooltip("The speed under which the projectile will not penetrate objects.")]
		private float m_PenetrationThreeshold = 20f;

		[SerializeField]
		private float m_PenetrationOffset = 0.2f;

		[SerializeField]
		private Vector2 m_RandomRotation;

		[Header("Twang")]
		[SerializeField]
		private float m_TwangDuration = 1f;

		[SerializeField]
		private float m_TwangRange = 18f;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_HitAudio;

		[SerializeField]
		private SoundPlayer m_TwangAudio;

		[SerializeField]
		private SoundType m_PenetrationType;

		private EntityEventHandler m_EntityThatLaunched;

		private Collider m_Collider;

		private Rigidbody m_Rigidbody;

		private bool m_Done;

		private bool m_Launched;

		public void Launch(EntityEventHandler entityThatLaunched)
		{
			if (m_Launched)
			{
				Debug.LogWarningFormat(this, "Already launched this projectile!", base.name);
			}
			else
			{
				m_EntityThatLaunched = entityThatLaunched;
				m_Rigidbody.linearVelocity = base.transform.forward * m_LaunchSpeed;
				m_Launched = true;
			}
		}

		private void Awake()
		{
			m_Collider = GetComponent<Collider>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Collider.enabled = false;
		}

		private void FixedUpdate()
		{
			if (m_Done)
			{
				return;
			}
			Ray ray = new Ray(base.transform.position, base.transform.forward);
			RaycastHit hitInfo;
			if (!Physics.Raycast(ray, out hitInfo, m_MaxDistance, m_Mask, QueryTriggerInteraction.Ignore))
			{
				return;
			}
			SurfaceData surfaceData = ScriptableSingleton<SurfaceDatabase>.Instance.GetSurfaceData(hitInfo);
			float magnitude = m_Rigidbody.linearVelocity.magnitude;
			if (magnitude >= m_PenetrationThreeshold && surfaceData != null)
			{
				surfaceData.PlaySound(ItemSelectionMethod.RandomlyButExcludeLast, m_PenetrationType, 1f, m_AudioSource);
				base.transform.SetParent(hitInfo.collider.transform, true);
				base.transform.position = hitInfo.point + base.transform.forward * m_PenetrationOffset;
				m_Pivot.localPosition = Vector3.back * m_PenetrationOffset;
				m_Model.SetParent(m_Pivot, true);
				float num = m_Rigidbody.mass * magnitude;
				IDamageable component = hitInfo.collider.GetComponent<IDamageable>();
				if (component != null)
				{
					float num2 = m_MaxDamage * m_DamageCurve.Evaluate(1f - magnitude / m_LaunchSpeed);
					HealthEventData damageData = new HealthEventData(0f - num2, m_EntityThatLaunched, hitInfo.point, ray.direction, num);
					component.ReceiveDamage(damageData);
				}
				else if ((bool)hitInfo.rigidbody)
				{
					hitInfo.rigidbody.AddForceAtPosition(base.transform.forward * num, base.transform.position, ForceMode.Impulse);
				}
				StartCoroutine(C_DoTwang());
				HitBox component2 = hitInfo.collider.GetComponent<HitBox>();
				if ((bool)component2)
				{
					Collider[] array = Physics.OverlapSphere(base.transform.position, 1.5f, m_Mask, QueryTriggerInteraction.Ignore);
					for (int i = 0; i < array.Length; i++)
					{
						HitBox component3 = array[i].GetComponent<HitBox>();
						if ((bool)component3)
						{
							Physics.IgnoreCollision(component3.Collider, m_Collider);
						}
					}
				}
				Physics.IgnoreCollision(m_Collider, hitInfo.collider);
				m_Rigidbody.isKinematic = true;
			}
			else
			{
				m_HitAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
				m_Rigidbody.isKinematic = false;
			}
			m_Collider.enabled = true;
			m_Collider.isTrigger = true;
			m_Done = true;
		}

		private IEnumerator C_DoTwang()
		{
			float stopTime = Time.time + m_TwangDuration;
			float range = m_TwangRange;
			float currentVelocity = 0f;
			Quaternion randomRotation = Quaternion.Euler(new Vector2(Random.Range(0f - m_RandomRotation.x, m_RandomRotation.x), Random.Range(0f - m_RandomRotation.y, m_RandomRotation.y)));
			while (Time.time < stopTime)
			{
				m_Pivot.localRotation = randomRotation * Quaternion.Euler(Random.Range(0f - range, range), Random.Range(0f - range, range), 0f);
				range = Mathf.SmoothDamp(range, 0f, ref currentVelocity, stopTime - Time.time);
				yield return null;
			}
		}
	}
}
