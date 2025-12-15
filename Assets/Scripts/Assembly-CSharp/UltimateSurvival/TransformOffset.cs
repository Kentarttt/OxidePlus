using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class TransformOffset
	{
		[SerializeField]
		private float m_LerpSpeed = 5f;

		[SerializeField]
		private Vector3 m_Position;

		[SerializeField]
		private Vector3 m_Rotation;

		private Vector3 m_CurrentPosition;

		private Vector3 m_CurrentRotation;

		public Vector3 CurrentPosition
		{
			get
			{
				return m_CurrentPosition;
			}
		}

		public Vector3 CurrentRotation
		{
			get
			{
				return m_CurrentRotation;
			}
		}

		public TransformOffset GetClone()
		{
			return (TransformOffset)MemberwiseClone();
		}

		public void Reset()
		{
			m_CurrentPosition = (m_CurrentRotation = Vector3.zero);
		}

		public void ContinueFrom(Vector3 position, Vector3 rotation)
		{
			m_CurrentPosition = position;
			m_CurrentRotation = rotation;
		}

		public void ContinueFrom(TransformOffset state)
		{
			m_CurrentPosition = state.CurrentPosition;
			m_CurrentRotation = state.CurrentRotation;
		}

		public void Update(float deltaTime, out Vector3 position, out Quaternion rotation)
		{
			m_CurrentPosition = Vector3.Lerp(m_CurrentPosition, m_Position, deltaTime * m_LerpSpeed);
			m_CurrentRotation = new Vector3(Mathf.LerpAngle(m_CurrentRotation.x, m_Rotation.x, deltaTime * m_LerpSpeed), Mathf.LerpAngle(m_CurrentRotation.y, m_Rotation.y, deltaTime * m_LerpSpeed), Mathf.LerpAngle(m_CurrentRotation.z, m_Rotation.z, deltaTime * m_LerpSpeed));
			position = m_CurrentPosition;
			rotation = Quaternion.Euler(m_CurrentRotation);
		}
	}
}
