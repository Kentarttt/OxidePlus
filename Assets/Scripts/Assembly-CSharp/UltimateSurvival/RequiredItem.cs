using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class RequiredItem
	{
		[SerializeField]
		private string m_Name;

		[SerializeField]
		private int m_Amount;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public int Amount
		{
			get
			{
				return m_Amount;
			}
		}
	}
}
