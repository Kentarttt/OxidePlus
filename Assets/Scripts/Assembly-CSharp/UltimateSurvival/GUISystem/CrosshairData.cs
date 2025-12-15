using System;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	[Serializable]
	public class CrosshairData
	{
		[SerializeField]
		private string m_ItemName;

		[SerializeField]
		private bool m_HideWhenAiming = true;

		[SerializeField]
		private Color m_NormalColor = Color.white;

		[SerializeField]
		private Color m_OnEntityColor = Color.red;

		[SerializeField]
		private CrosshairType m_Type;

		[SerializeField]
		private Image m_Image;

		[SerializeField]
		private Sprite m_Sprite;

		[SerializeField]
		private Vector2 m_Size = new Vector2(64f, 64f);

		[SerializeField]
		private DynamicCrosshair m_Crosshair;

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_IdleDistance = 32f;

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_CrouchDistance = 24f;

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_WalkDistance = 36f;

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_RunDistance = 48f;

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_JumpDistance = 54f;

		public string ItemName
		{
			get
			{
				return m_ItemName;
			}
		}

		public bool HideWhenAiming
		{
			get
			{
				return m_HideWhenAiming;
			}
		}

		public static implicit operator bool(CrosshairData cd)
		{
			return cd != null;
		}

		public void Update(PlayerEventHandler player)
		{
			RaycastData raycastData = player.RaycastData.Get();
			bool flag = false;
			if ((bool)raycastData && (bool)raycastData.HitInfo.collider && (bool)raycastData.HitInfo.collider.GetComponent<HitBox>())
			{
				flag = true;
			}
			if (m_Type == CrosshairType.Dynamic && (bool)m_Crosshair)
			{
				m_Crosshair.SetColor((!flag) ? m_NormalColor : m_OnEntityColor);
				float b = m_IdleDistance;
				if (player.Crouch.Active)
				{
					b = m_CrouchDistance;
				}
				else if (player.Walk.Active)
				{
					b = m_WalkDistance;
				}
				else if (player.Run.Active)
				{
					b = m_RunDistance;
				}
				else if (player.Jump.Active)
				{
					b = m_JumpDistance;
				}
				m_Crosshair.SetDistance(Mathf.Lerp(m_Crosshair.Distance, b, Time.deltaTime * 10f));
			}
			else if (m_Type == CrosshairType.Simple && (bool)m_Image)
			{
				m_Image.color = ((!flag) ? m_NormalColor : m_OnEntityColor);
			}
		}

		public void SetActive(bool active)
		{
			if (m_Type == CrosshairType.Dynamic && (bool)m_Crosshair)
			{
				m_Crosshair.SetActive(active);
			}
			else if (m_Type == CrosshairType.Simple && (bool)m_Image)
			{
				m_Image.enabled = active;
				m_Image.sprite = m_Sprite;
				m_Image.rectTransform.sizeDelta = m_Size;
			}
		}
	}
}
