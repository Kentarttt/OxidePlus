using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class Sway
	{
		[SerializeField]
		private bool Enabled;

		[SerializeField]
		private Vector2 Magnitude;

		[SerializeField]
		private float LerpSpeed;

		public Vector2 Value { get; private set; }

		public Sway GetClone()
		{
			return (Sway)MemberwiseClone();
		}

		public void CalculateSway(Vector2 input, float deltaTime)
		{
			if (Enabled)
			{
				Vector2 magnitude = Magnitude;
				magnitude.Scale(input);
				Value = Vector2.Lerp(Value, magnitude, deltaTime * LerpSpeed);
			}
		}
	}
}
