using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class BuildingCategory : Selectable
	{
		public BuildingMenu BuildingMenu;

		public int CategoryIndex;

		[SerializeField]
		private string m_CategoryName;

		public GameObject m_PiecesParent;

		public string CategoryName
		{
			get
			{
				return m_CategoryName;
			}
		}

		public void ShowPieces(bool show)
		{
			if ((bool)m_PiecesParent)
			{
				m_PiecesParent.SetActive(show);
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			BuildingMenu.SelectCategory(this);
		}
	}
}
