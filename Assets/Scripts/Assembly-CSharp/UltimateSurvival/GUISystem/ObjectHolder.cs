using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.GUISystem
{
	public class ObjectHolder
	{
		private string m_Name;

		private List<GameObject> m_ObjectList;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public List<GameObject> ObjectList
		{
			get
			{
				return m_ObjectList;
			}
		}

		public ObjectHolder(string name, List<GameObject> objectList)
		{
			m_Name = name;
			m_ObjectList = objectList;
		}

		public static implicit operator bool(ObjectHolder holder)
		{
			return holder != null;
		}

		public void ActivateObjects(bool active)
		{
			for (int i = 0; i < m_ObjectList.Count; i++)
			{
				m_ObjectList[i].SetActive(active);
			}
		}
	}
}
