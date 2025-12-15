using UnityEngine;

namespace UltimateSurvival
{
	public static class CameraShakePresets
	{
		public static ShakeInstance Bump
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(2.5f, 4f, 0.1f, 0.75f);
				shakeInstance.PositionInfluence = Vector3.one * 0.15f;
				shakeInstance.RotationInfluence = Vector3.one;
				return shakeInstance;
			}
		}

		public static ShakeInstance Explosion
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(5f, 10f, 0f, 1.5f);
				shakeInstance.PositionInfluence = Vector3.one * 0.25f;
				shakeInstance.RotationInfluence = new Vector3(4f, 1f, 1f);
				return shakeInstance;
			}
		}

		public static ShakeInstance Earthquake
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(0.6f, 3.5f, 2f, 10f);
				shakeInstance.PositionInfluence = Vector3.one * 0.25f;
				shakeInstance.RotationInfluence = new Vector3(1f, 1f, 4f);
				return shakeInstance;
			}
		}

		public static ShakeInstance BadTrip
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(10f, 0.15f, 5f, 10f);
				shakeInstance.PositionInfluence = new Vector3(0f, 0f, 0.15f);
				shakeInstance.RotationInfluence = new Vector3(2f, 1f, 4f);
				return shakeInstance;
			}
		}

		public static ShakeInstance HandheldCamera
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(1f, 0.25f, 5f, 10f);
				shakeInstance.PositionInfluence = Vector3.zero;
				shakeInstance.RotationInfluence = new Vector3(1f, 0.5f, 0.5f);
				return shakeInstance;
			}
		}

		public static ShakeInstance Vibration
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(0.4f, 20f, 2f, 2f);
				shakeInstance.PositionInfluence = new Vector3(0f, 0.15f, 0f);
				shakeInstance.RotationInfluence = new Vector3(1.25f, 0f, 4f);
				return shakeInstance;
			}
		}

		public static ShakeInstance RoughDriving
		{
			get
			{
				ShakeInstance shakeInstance = new ShakeInstance(1f, 2f, 1f, 1f);
				shakeInstance.PositionInfluence = Vector3.zero;
				shakeInstance.RotationInfluence = Vector3.one;
				return shakeInstance;
			}
		}
	}
}
