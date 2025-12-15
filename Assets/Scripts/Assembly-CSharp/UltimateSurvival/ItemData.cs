using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class ItemData
	{
		[SerializeField]
		private string m_Name;

		[SerializeField]
		private string m_DisplayName;

		[SerializeField]
		private int m_Id;

		[SerializeField]
		private string m_Category;

		[SerializeField]
		private Sprite m_Icon;

		[SerializeField]
		private GameObject m_WorldObject;

		[SerializeField]
		[Multiline]
		private string[] m_Descriptions;

		[SerializeField]
		private int m_StackSize = 1;

		[SerializeField]
		private List<ItemProperty.Value> m_PropertyValues;

		[SerializeField]
		private bool m_IsBuildable;

		[SerializeField]
		private bool m_IsCraftable;

		[SerializeField]
		private Recipe m_Recipe;

		[SerializeField]
		private Purchase m_Purchase;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public string DisplayName
		{
			get
			{
				return m_DisplayName;
			}
		}

		public int Id
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;
			}
		}

		public string Category
		{
			get
			{
				return m_Category;
			}
			set
			{
				m_Category = value;
			}
		}

		public Sprite Icon
		{
			get
			{
				return m_Icon;
			}
		}

		public GameObject WorldObject
		{
			get
			{
				return m_WorldObject;
			}
		}

		public string[] Descriptions
		{
			get
			{
				return m_Descriptions;
			}
		}

		public int StackSize
		{
			get
			{
				return m_StackSize;
			}
		}

		public List<ItemProperty.Value> PropertyValues
		{
			get
			{
				return m_PropertyValues;
			}
		}

		public bool IsBuildable
		{
			get
			{
				return m_IsBuildable;
			}
		}

		public bool IsCraftable
		{
			get
			{
				return m_IsCraftable;
			}
		}

		public Recipe Recipe
		{
			get
			{
				return m_Recipe;
			}
		}

		public Purchase Purchase
		{
			get
			{
				return m_Purchase;
			}
		}
	}
}
