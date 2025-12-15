using UnityEngine;

namespace UltimateSurvival
{
	public class RaycastManager : PlayerBehaviour
	{
		[SerializeField]
		private Camera m_WorldCamera;

		[SerializeField]
		[Tooltip("The maximum distance at which you can interact with objects.")]
		private float m_RayLength = 1.5f;

		[SerializeField]
		[Tooltip("The distance at which an object is considered 'too close'.")]
		private float m_TooCloseThreeshold = 1f;

		[SerializeField]
		private LayerMask m_LayerMask;

		private void Start()
		{
			base.Player.InteractOnce.SetTryer(Try_InteractOnce);
			base.Player.Sleep.AddStartListener(delegate
			{
				base.Player.RaycastData.Set(null);
			});
		}

		private bool Try_InteractOnce()
		{
			RaycastData raycastData = base.Player.RaycastData.Get();
			if ((bool)raycastData && raycastData.ObjectIsInteractable)
			{
				raycastData.InteractableObject.OnInteract(base.Player);
			}
			return true;
		}

		private void Update()
		{
			Ray ray = m_WorldCamera.ViewportPointToRay(Vector2.one * 0.5f);
			RaycastData raycastData = base.Player.RaycastData.Get();
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, m_RayLength, m_LayerMask, QueryTriggerInteraction.Ignore))
			{
				RaycastData raycastData2 = new RaycastData(hitInfo);
				base.Player.RaycastData.Set(raycastData2);
				if ((bool)raycastData && raycastData2.ObjectIsInteractable && raycastData2.InteractableObject != raycastData.InteractableObject)
				{
					raycastData2.InteractableObject.OnRaycastStart(base.Player);
				}
				else if (raycastData2.ObjectIsInteractable)
				{
					raycastData2.InteractableObject.OnRaycastUpdate(base.Player);
				}
			}
			else
			{
				if ((bool)raycastData && raycastData.ObjectIsInteractable)
				{
					raycastData.InteractableObject.OnRaycastEnd(base.Player);
				}
				if (raycastData != null)
				{
					base.Player.RaycastData.Set(null);
				}
			}
			bool value = (bool)base.Player.RaycastData.Get() && base.Player.RaycastData.Get().HitInfo.distance < m_TooCloseThreeshold;
			base.Player.IsCloseToAnObject.Set(value);
		}
	}
}
