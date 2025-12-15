using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class Recipe
	{
		[SerializeField]
		[Range(1f, 999f)]
		private int m_Duration = 1;

		[SerializeField]
		private RequiredItem[] m_RequiredItems;

		public int Duration
		{
			get
			{
				return m_Duration;
			}
		}

		public RequiredItem[] RequiredItems
		{
			get
			{
				return m_RequiredItems;
			}
		}

		public static implicit operator bool(Recipe recipe)
		{
			return recipe != null;
		}
	}
}
