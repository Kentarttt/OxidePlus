using UnityEngine;

namespace UltimateSurvival
{
	public class Gunshot
	{
		public Vector3 Position { get; private set; }

		public EntityEventHandler EntityThatShot { get; private set; }

		public Gunshot(Vector3 position, EntityEventHandler entityThatShot = null)
		{
			Position = position;
			EntityThatShot = entityThatShot;
		}
	}
}
