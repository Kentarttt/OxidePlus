using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class PlayerDeathHandler : PlayerBehaviour
	{
		[SerializeField]
		private GameObject m_Camera;

		[Header("Audio")]
		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		private SoundPlayer m_DeathAudio;

		[Header("Stuff To Disable On Death")]
		[SerializeField]
		private GameObject[] m_ObjectsToDisable;

		[SerializeField]
		private Behaviour[] m_BehavioursToDisable;

		[SerializeField]
		private Collider[] m_CollidersToDisable;

		[Header("Ragdoll")]
		[SerializeField]
		private bool m_EnableRagdoll;

		[SerializeField]
		[Tooltip("A Ragdoll component, usually attached to the armature of the character.")]
		private Ragdoll m_Ragdoll;

		[Header("Respawn")]
		[SerializeField]
		private bool m_AutoRespawn = true;

		[SerializeField]
		private float m_RespawnDuration = 10f;

		[SerializeField]
		private float m_RespawnBlockTime = 3f;

		private Vector3 m_CamStartPos;

		private Quaternion m_CamStartRot;

		private void Awake()
		{
			if (m_EnableRagdoll && !m_Ragdoll)
			{
				Debug.LogError("The ragdoll option has been enabled but no ragdoll object is assigned!", this);
			}
			base.Player.Health.AddChangeListener(OnChanged_Health);
			m_CamStartPos = m_Camera.transform.localPosition;
			m_CamStartRot = m_Camera.transform.localRotation;
		}

		private void OnChanged_Health()
		{
			if (base.Player.Health.Is(0f))
			{
				On_Death();
				Ray ray = new Ray(base.transform.position + Vector3.up, Vector3.down);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, 1.5f, -1))
				{
					m_Camera.transform.position = hitInfo.point + Vector3.up * 0.1f;
					m_Camera.transform.rotation = Quaternion.Euler(-30f, Random.Range(-180f, 180f), 0f);
				}
			}
		}

		private void On_Death()
		{
			m_DeathAudio.Play(ItemSelectionMethod.Randomly, m_AudioSource);
			if (m_EnableRagdoll && (bool)m_Ragdoll)
			{
				m_Ragdoll.Enable();
			}
			GameObject[] objectsToDisable = m_ObjectsToDisable;
			foreach (GameObject gameObject in objectsToDisable)
			{
				gameObject.SetActive(false);
			}
			Behaviour[] behavioursToDisable = m_BehavioursToDisable;
			foreach (Behaviour behaviour in behavioursToDisable)
			{
				behaviour.enabled = false;
			}
			Collider[] collidersToDisable = m_CollidersToDisable;
			foreach (Collider collider in collidersToDisable)
			{
				collider.enabled = false;
			}
			if (m_AutoRespawn)
			{
				StartCoroutine(C_Respawn());
			}
			base.Player.Death.Send();
		}

		private IEnumerator C_Respawn()
		{
			yield return new WaitForSeconds(m_RespawnDuration);
			if (m_EnableRagdoll && (bool)m_Ragdoll)
			{
				m_Ragdoll.Disable();
			}
			m_Camera.transform.localPosition = m_CamStartPos;
			m_Camera.transform.localRotation = m_CamStartRot;
			if (base.Player.LastSleepPosition.Get() != Vector3.zero)
			{
				base.transform.position = base.Player.LastSleepPosition.Get();
				base.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(-180f, 180f));
			}
			else
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Spawn Point");
				if (array != null && array.Length > 0)
				{
					GameObject gameObject = array[Random.Range(0, array.Length)];
					base.transform.position = gameObject.transform.position;
					base.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(-180f, 180f));
				}
			}
			yield return new WaitForSeconds(m_RespawnBlockTime);
			GameObject[] objectsToDisable = m_ObjectsToDisable;
			foreach (GameObject gameObject2 in objectsToDisable)
			{
				gameObject2.SetActive(true);
			}
			Behaviour[] behavioursToDisable = m_BehavioursToDisable;
			foreach (Behaviour behaviour in behavioursToDisable)
			{
				behaviour.enabled = true;
			}
			Collider[] collidersToDisable = m_CollidersToDisable;
			foreach (Collider collider in collidersToDisable)
			{
				collider.enabled = true;
			}
			base.Player.Health.Set(100f);
			base.Player.Thirst.Set(100f);
			base.Player.Hunger.Set(100f);
			base.Player.Stamina.Set(100f);
			int added;
			MonoSingleton<InventoryController>.Instance.AddItemToCollection(23, 1, "Hotbar", out added);
			base.Player.Respawn.Send();
		}
	}
}
