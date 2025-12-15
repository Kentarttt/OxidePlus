using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class Purchase
	{
		[SerializeField]
		[Range(0f, 999f)]
		private int m_Price;

		[SerializeField]
		[Range(0f, 999f)]
		private int m_Amount;

		public int Price
		{
			get
			{
				return m_Price;
			}
		}

		public int Amount
		{
			get
			{
				return m_Amount;
			}
		}

		public static implicit operator bool(Purchase purchase)
		{
			return purchase != null;
		}
	}
}
