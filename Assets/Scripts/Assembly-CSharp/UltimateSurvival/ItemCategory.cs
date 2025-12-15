using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class ItemCategory
	{
		[SerializeField]
		private string m_Name;

		[SerializeField]
		private ItemData[] m_Items;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public ItemData[] Items
		{
			get
			{
				return m_Items;
			}
		}
	}
}
