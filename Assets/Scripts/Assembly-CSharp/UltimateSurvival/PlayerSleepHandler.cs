using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class PlayerSleepHandler : PlayerBehaviour
	{
		[SerializeField]
		private float m_SleepSpeed = 0.33f;

		[SerializeField]
		[Tooltip("How much time to wait after the sleep is done, before getting up.")]
		private float m_SleepFinishPause = 2f;

		[SerializeField]
		[Range(4f, 12f)]
		private int m_GetUpHour = 8;

		[Header("Stuff To Disable When Sleeping")]
		[SerializeField]
		private Collider[] m_CollidersToDisable;

		[SerializeField]
		private GameObject[] m_ObjectsToDisable;

		[SerializeField]
		private Behaviour[] m_BehavioursToDisable;

		private Vector3 m_BeforeSleepPosition;

		private Quaternion m_BeforeSleepRotation;

		private void Awake()
		{
			base.Player.StartSleeping.SetTryer(Try_StartSleeping);
		}

		private bool Try_StartSleeping(SleepingBag bag)
		{
			if (base.Player.Sleep.Active)
			{
				return false;
			}
			StartCoroutine(C_Sleep(bag));
			return true;
		}

		private IEnumerator C_Sleep(SleepingBag bag)
		{
			m_BeforeSleepPosition = base.transform.position;
			m_BeforeSleepRotation = base.transform.rotation;
			EnableStuff(false);
			int hoursToSleep = 0;
			float hoursSlept = 0f;
			float speed2 = m_SleepSpeed;
			int currentHour = MonoSingleton<TimeOfDay>.Instance.CurrentHour;
			hoursToSleep = ((currentHour > 24 || currentHour <= 18) ? (m_GetUpHour - currentHour) : (24 - currentHour + m_GetUpHour));
			while (Mathf.Abs(MonoSingleton<TimeOfDay>.Instance.CurrentHour - m_GetUpHour) > 0)
			{
				MonoSingleton<TimeOfDay>.Instance.NormalizedTime += Time.deltaTime * speed2;
				hoursSlept += Time.deltaTime * speed2 * 24f;
				speed2 = Mathf.Lerp(m_SleepSpeed, 0f, hoursSlept / (float)hoursToSleep);
				speed2 = Mathf.Max(speed2, 0.001f);
				base.transform.position = Vector3.Lerp(base.transform.position, bag.SleepPosition, Time.deltaTime * 10f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, bag.SleepRotation, Time.deltaTime * 10f);
				yield return null;
			}
			yield return new WaitForSeconds(m_SleepFinishPause);
			while ((base.transform.position - m_BeforeSleepPosition).sqrMagnitude > 0.0001f && Quaternion.Angle(base.transform.rotation, m_BeforeSleepRotation) > 0.001f)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, m_BeforeSleepPosition, Time.deltaTime * 10f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, m_BeforeSleepRotation, Time.deltaTime * 10f);
				yield return null;
			}
			base.transform.position = m_BeforeSleepPosition;
			base.transform.rotation = m_BeforeSleepRotation;
			EnableStuff(true);
			base.Player.LastSleepPosition.Set(bag.SpawnPosOffset);
			base.Player.Sleep.ForceStop();
		}

		private void EnableStuff(bool enable)
		{
			GameObject[] objectsToDisable = m_ObjectsToDisable;
			foreach (GameObject gameObject in objectsToDisable)
			{
				gameObject.SetActive(enable);
			}
			Behaviour[] behavioursToDisable = m_BehavioursToDisable;
			foreach (Behaviour behaviour in behavioursToDisable)
			{
				behaviour.enabled = enable;
			}
			Collider[] collidersToDisable = m_CollidersToDisable;
			foreach (Collider collider in collidersToDisable)
			{
				collider.enabled = enable;
			}
		}
	}
}
