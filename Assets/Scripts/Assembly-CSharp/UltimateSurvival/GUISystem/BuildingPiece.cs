using UltimateSurvival.Building;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class BuildingPiece : Selectable
	{
		public BuildingMenu BuildingMenu;

		[SerializeField]
		private string m_PieceName;

		[SerializeField]
		private Vector2 m_DesiredOffset;

		[SerializeField]
		private UltimateSurvival.Building.BuildingPiece m_BuildableObject;

		public string PieceName
		{
			get
			{
				return m_PieceName;
			}
		}

		public UltimateSurvival.Building.BuildingPiece BuildableObject
		{
			get
			{
				return m_BuildableObject;
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			BuildingMenu.SelectPiece(this);
		}
	}
}
