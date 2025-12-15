using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UltimateSurvival.InputSystem
{
	public class Joystick : NetworkBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler, IEventSystemHandler
	{
		[SerializeField]
		[Tooltip("How far this can be moved(in pixels).")]
		private float m_MovementRange = 48f;

		private Canvas m_ParentCanvas;

		private Vector3 m_StartPosition;

		private Vector3 m_CurrentPosition;

		private float m_InitialMovementRange;

		private void Start()
		{
			m_ParentCanvas = GetComponentInParent<Canvas>();
			m_StartPosition = base.transform.position;
			m_InitialMovementRange = m_MovementRange;
		}

		public void OnDrag(PointerEventData data)
		{
			if (!isLocalPlayer) return;
		
			m_MovementRange = m_InitialMovementRange * m_ParentCanvas.scaleFactor;
			m_CurrentPosition.x = (int)(data.position.x - m_StartPosition.x);
			m_CurrentPosition.y = (int)(data.position.y - m_StartPosition.y);
			m_CurrentPosition = Vector3.ClampMagnitude(m_CurrentPosition, m_MovementRange);
			base.transform.position = m_StartPosition + m_CurrentPosition;
		}

		public void OnPointerUp(PointerEventData data)
		{
			if (!isLocalPlayer) return;
		
			base.transform.position = m_StartPosition;
			m_CurrentPosition = Vector3.zero;
		}

		public void OnPointerDown(PointerEventData data)
        {
            if (!isLocalPlayer) return;
        }

		public float GetHorizontalInput()
		{
			return m_CurrentPosition.x / m_MovementRange;
		}

		public float GetVerticalInput()
		{
			return m_CurrentPosition.y / m_MovementRange;
		}

		public float GetNormalizedMagnitude()
		{
			return m_CurrentPosition.magnitude / m_MovementRange;
		}
	}
}
