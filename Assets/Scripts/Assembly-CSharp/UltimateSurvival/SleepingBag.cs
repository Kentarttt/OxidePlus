using UnityEngine;

namespace UltimateSurvival
{
	public class SleepingBag : InteractableObject
	{
		[SerializeField]
		[Tooltip("The player spawn position offset, relative to this object.")]
		private Vector3 m_SpawnPosOffset = new Vector3(0f, 0.3f, 0f);

		[SerializeField]
		[Tooltip("Player sleep position, relative to this object.")]
		private Vector3 m_SleepPosOffset;

		[SerializeField]
		[Tooltip("Player sleep rotation, relative to this object.")]
		private Vector3 m_SleepRotOffset;

		public Vector3 SpawnPosOffset
		{
			get
			{
				return base.transform.position + base.transform.TransformVector(m_SpawnPosOffset);
			}
		}

		public Vector3 SleepPosition
		{
			get
			{
				return base.transform.position + base.transform.TransformVector(m_SleepPosOffset);
			}
		}

		public Quaternion SleepRotation
		{
			get
			{
				return base.transform.rotation * Quaternion.Euler(m_SleepRotOffset);
			}
		}

		public override void OnInteract(PlayerEventHandler player)
		{
			if (!player.Sleep.Active && MonoSingleton<TimeOfDay>.Instance.State.Get() == ET.TimeOfDay.Night && player.StartSleeping.Try(this))
			{
				player.Sleep.ForceStart();
			}
		}
	}
}
