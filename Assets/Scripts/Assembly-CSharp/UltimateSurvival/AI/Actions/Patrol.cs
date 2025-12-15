using System;
using UnityEngine;

namespace UltimateSurvival.AI.Actions
{
	[Serializable]
	public class Patrol : PointBased
	{
		[SerializeField]
		private Vector2 m_WaitTime = new Vector2(8f, 15f);

		[Header("Audio")]
		[SerializeField]
		private SoundPlayer m_Audio;

		[SerializeField]
		[Clamp(0f, 15f)]
		private float m_AudioPlayInterval = 1f;

		private float m_NextMoveTime;

		private bool m_Waiting;

		private Vector3? m_SuspectedTarget = Vector3.zero;

		private AIBrain m_Brain;

		private float m_LastAudioPlayTime;

		public override void OnStart(AIBrain brain)
		{
			m_Priority = 0;
			m_IsInterruptable = true;
			m_RepeatType = ET.ActionRepeatType.Repetitive;
			base.OnStart(brain);
			m_Brain = brain;
		}

		public override void Activate(AIBrain brain)
		{
			brain.Settings.Movement.MoveTo(m_PointPositions[m_CurrentIndex]);
			brain.AI.ChangeHealth.AddListener(OnSucceded_ChangeHealth);
			GameController.Audio.LastGunshot.AddChangeListener(OnChanged_LastGunshot);
		}

		public override void OnUpdate(AIBrain brain)
		{
			if (m_SuspectedTarget.HasValue)
			{
				if (brain.Settings.Movement.CurrentDestination != m_SuspectedTarget.Value)
				{
					brain.Settings.Movement.MoveTo(m_SuspectedTarget.Value);
				}
				if (brain.Settings.Movement.ReachedDestination())
				{
					m_SuspectedTarget = null;
				}
			}
			else if (brain.Settings.Movement.ReachedDestination())
			{
				if (!m_Waiting)
				{
					m_Waiting = true;
					m_NextMoveTime = Time.time + UnityEngine.Random.Range(m_WaitTime.x, m_WaitTime.y);
				}
				if (Time.time > m_NextMoveTime)
				{
					ChangePatrolPoint();
					brain.Settings.Movement.MoveTo(m_PointPositions[m_CurrentIndex], m_IsUrgent);
					m_Waiting = false;
				}
			}
			if (Time.time > m_LastAudioPlayTime + m_AudioPlayInterval)
			{
				m_LastAudioPlayTime = Time.time;
				m_Audio.Play(ItemSelectionMethod.RandomlyButExcludeLast, brain.Settings.AudioSource);
			}
		}

		public override void OnDeactivation(AIBrain brain)
		{
			brain.Settings.Animation.ToggleBool("Walk", false);
			brain.Settings.Animation.ToggleBool("Run", false);
			brain.AI.ChangeHealth.RemoveListener(OnSucceded_ChangeHealth);
			GameController.Audio.LastGunshot.RemoveChangeListener(OnChanged_LastGunshot);
			brain.Settings.AudioSource.Stop();
		}

		public override bool IsDone(AIBrain brain)
		{
			return false;
		}

		private void OnSucceded_ChangeHealth(HealthEventData data)
		{
			if (data.Damager != null)
			{
				m_SuspectedTarget = data.Damager.transform.position;
			}
		}

		private void OnChanged_LastGunshot()
		{
			if (!(m_Brain == null))
			{
				Gunshot gunshot = GameController.Audio.LastGunshot.Get();
				float sqrMagnitude = (gunshot.Position - m_Brain.transform.position).sqrMagnitude;
				if (sqrMagnitude < (float)(m_Brain.Settings.Detection.HearRange * m_Brain.Settings.Detection.HearRange))
				{
					m_SuspectedTarget = gunshot.Position;
				}
			}
		}
	}
}
