using System.Collections.Generic;
using CnControls;
using UnityEngine;

namespace UltimateSurvival
{
	public class MouseLook : PlayerBehaviour
	{
		[Header("General")]
		[SerializeField]
		[Tooltip("The camera root which will be rotated up & down (on the X axis).")]
		private Transform m_LookRoot;

		[SerializeField]
		private Transform m_PlayerRoot;

		[SerializeField]
		[Tooltip("The up & down rotation will be inverted, if checked.")]
		private bool m_Invert;

		[Header("Motion")]
		[SerializeField]
		[Tooltip("The higher it is, the faster the camera will rotate.")]
		private float m_Sensitivity = 5f;

		[SerializeField]
		[Range(0f, 20f)]
		private int m_SmoothSteps = 10;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_SmoothWeight = 0.4f;

		[SerializeField]
		private float m_RollAngle = 10f;

		[SerializeField]
		private float m_RollSpeed = 3f;

		[Header("Rotation Limits")]
		[SerializeField]
		private Vector2 m_DefaultLookLimits = new Vector2(-60f, 90f);

		private float m_CurrentRollAngle;

		private bool m_InventoryIsOpen;

		private Vector2 m_LookAngles;

		private int m_LastLookFrame;

		private Vector2 m_CurrentMouseLook;

		private Vector2 m_SmoothMove;

		private List<Vector2> m_SmoothBuffer = new List<Vector2>();

		public bool lockState;

		private float sensitivity = 1f;
		private string playerPrefsKey = "MySliderValue";

		private void Start()
		{
			if (!m_LookRoot)
			{
				Debug.LogErrorFormat(this, "Assign the look root in the inspector!", base.name);
				base.enabled = false;
			}
			MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnChanged_InventoryState);
			sensitivity = PlayerPrefs.GetFloat(playerPrefsKey + "Sensitivity", 1f);
		}

		private void OnChanged_InventoryState()
		{
			m_InventoryIsOpen = !MonoSingleton<InventoryController>.Instance.IsClosed;
			if (m_InventoryIsOpen)
			{
				lockState = true;
			}
			else
			{
				lockState = false;
			}
		}

		private void Update()
		{
			if (base.Player.ViewLocked.Is(false) && !lockState && !base.Player.Sleep.Active && base.Player.Health.Get() > 0f)
			{
				LookAround();
			}
			base.Player.ViewLocked.Set(lockState || base.Player.SelectBuildable.Active);
		}

		private void LookAround()
		{
			CalculateMouseInput(Time.deltaTime);
			m_LookAngles.x += m_CurrentMouseLook.x * m_Sensitivity * ((!m_Invert) ? (-1f) : 1f);
			m_LookAngles.y += m_CurrentMouseLook.y * m_Sensitivity;
			m_LookAngles.x = ClampAngle(m_LookAngles.x, m_DefaultLookLimits.x, m_DefaultLookLimits.y);
			m_CurrentRollAngle = Mathf.Lerp(m_CurrentRollAngle, base.Player.LookInput.Get().x * m_RollAngle, Time.deltaTime * m_RollSpeed);
			m_LookRoot.localRotation = Quaternion.Euler(m_LookAngles.x, 0f, m_CurrentRollAngle);
			m_PlayerRoot.localRotation = Quaternion.Euler(0f, m_LookAngles.y, 0f);
			base.Player.LookDirection.Set(m_LookRoot.forward);
		}

		private float ClampAngle(float angle, float min, float max)
		{
			if (angle > 360f)
			{
				angle -= 360f;
			}
			else if (angle < -360f)
			{
				angle += 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		private void CalculateMouseInput(float deltaTime)
		{
			if (m_LastLookFrame != Time.frameCount)
			{
				m_LastLookFrame = Time.frameCount;
				m_SmoothMove = new Vector2(CnInputManager.GetAxisRaw("Mouse Y") * sensitivity, CnInputManager.GetAxisRaw("Mouse X") * sensitivity);
				m_SmoothSteps = Mathf.Clamp(m_SmoothSteps, 1, 20);
				m_SmoothWeight = Mathf.Clamp01(m_SmoothWeight);
				while (m_SmoothBuffer.Count > m_SmoothSteps)
				{
					m_SmoothBuffer.RemoveAt(0);
				}
				m_SmoothBuffer.Add(m_SmoothMove);
				float num = 1f;
				Vector2 zero = Vector2.zero;
				float num2 = 0f;
				for (int num3 = m_SmoothBuffer.Count - 1; num3 > 0; num3--)
				{
					zero += m_SmoothBuffer[num3] * num;
					num2 += num;
					num *= m_SmoothWeight / (deltaTime * 60f);
				}
				num2 = Mathf.Max(1f, num2);
				m_CurrentMouseLook = zero / num2;
			}
		}
	}
}
