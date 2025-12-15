using UnityEngine;

namespace UltimateSurvival
{
	public class RaycastData
	{
		public bool ObjectIsInteractable { get; private set; }

		public GameObject GameObject { get; private set; }

		public InteractableObject InteractableObject { get; private set; }

		public RaycastHit HitInfo { get; private set; }

		public RaycastData(RaycastHit hitInfo)
		{
			GameObject = hitInfo.collider.gameObject;
			InteractableObject = hitInfo.collider.GetComponent<InteractableObject>();
			if ((bool)InteractableObject && !InteractableObject.enabled)
			{
				InteractableObject = null;
			}
			ObjectIsInteractable = InteractableObject != null;
			HitInfo = hitInfo;
		}

		public static implicit operator bool(RaycastData raycastData)
		{
			return !object.ReferenceEquals(raycastData, null);
		}
	}
}
