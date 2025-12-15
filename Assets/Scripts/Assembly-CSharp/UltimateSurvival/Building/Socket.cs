using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.Building
{
	public class Socket : MonoBehaviour
	{
		[Serializable]
		public class PieceOffset
		{
			[SerializeField]
			private BuildingPiece m_Piece;

			[SerializeField]
			private Vector3 m_PositionOffset = Vector3.one;

			[SerializeField]
			private Vector3 m_RotationOffset;

			public BuildingPiece Piece
			{
				get
				{
					return m_Piece;
				}
				set
				{
					m_Piece = value;
				}
			}

			public Vector3 PositionOffset
			{
				get
				{
					return m_PositionOffset;
				}
				set
				{
					m_PositionOffset = value;
				}
			}

			public Quaternion RotationOffset
			{
				get
				{
					return Quaternion.Euler(m_RotationOffset);
				}
			}

			public Vector3 RotationOffsetEuler
			{
				get
				{
					return m_RotationOffset;
				}
				set
				{
					m_RotationOffset = value;
				}
			}

			public PieceOffset GetMemberwiseClone()
			{
				return (PieceOffset)MemberwiseClone();
			}
		}

		public class SpaceOccupier
		{
			public BuildingSpace OccupiedSpace { get; private set; }

			public BuildingPiece Occupier { get; private set; }

			public SpaceOccupier(BuildingSpace occupiedSpace, BuildingPiece occupier)
			{
				OccupiedSpace = occupiedSpace;
				Occupier = occupier;
			}
		}

		[SerializeField]
		private List<PieceOffset> m_PieceOffsets;

		[SerializeField]
		private float m_Radius = 1f;

		private BuildingPiece m_Piece;

		private List<BuildingSpace> m_OccupiedSpaces = new List<BuildingSpace>();

		private List<SpaceOccupier> m_Occupiers = new List<SpaceOccupier>();

		public List<BuildingSpace> OccupiedSpaces
		{
			get
			{
				return m_OccupiedSpaces;
			}
		}

		public BuildingPiece Piece
		{
			get
			{
				return m_Piece;
			}
		}

		public List<PieceOffset> PieceOffsets
		{
			get
			{
				return m_PieceOffsets;
			}
			set
			{
				m_PieceOffsets = value;
			}
		}

		public float Radius
		{
			get
			{
				return m_Radius;
			}
			set
			{
				m_Radius = value;
			}
		}

		private void Awake()
		{
			SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			sphereCollider.isTrigger = true;
			sphereCollider.radius = Radius;
			m_Piece = GetComponentInParent<BuildingPiece>();
		}

		public void OnPieceDeath(BuildingPiece piece)
		{
			for (int i = 0; i < m_Occupiers.Count; i++)
			{
				if (piece == m_Occupiers[i].Occupier)
				{
					m_OccupiedSpaces.RemoveAt(i);
					m_Occupiers.RemoveAt(i);
				}
			}
		}

		public bool GetPieceOffsetByName(string name, out PieceOffset offset)
		{
			offset = new PieceOffset();
			for (int i = 0; i < m_PieceOffsets.Count; i++)
			{
				if (m_PieceOffsets[i].Piece != null && m_PieceOffsets[i].Piece.Name == name)
				{
					offset = m_PieceOffsets[i];
					return true;
				}
			}
			return false;
		}

		public bool HasSpace(LayerMask mask, BuildingPiece placedPiece)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, Radius, mask, QueryTriggerInteraction.Ignore);
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (m_Piece != placedPiece)
				{
					if (!m_Piece.Building.HasCollider(collider) && collider as TerrainCollider == null)
					{
						return false;
					}
				}
				else if (!m_Piece.HasCollider(collider) && collider as TerrainCollider == null)
				{
					return false;
				}
			}
			return true;
		}

		public void OccupySpaces(BuildingSpace[] spacesToOccupy, BuildingPiece piece)
		{
			for (int i = 0; i < spacesToOccupy.Length; i++)
			{
				if (!m_OccupiedSpaces.Contains(spacesToOccupy[i]))
				{
					m_OccupiedSpaces.Add(spacesToOccupy[i]);
					m_Occupiers.Add(new SpaceOccupier(spacesToOccupy[i], piece));
				}
			}
		}

		public void OccupyNeighbours(LayerMask freePlacementMask, LayerMask buildingMask, BuildingPiece placedPiece)
		{
			Collider[] array = Physics.OverlapBox(placedPiece.Bounds.center, placedPiece.Bounds.extents, placedPiece.transform.rotation, freePlacementMask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				Socket component = array[i].GetComponent<Socket>();
				if ((bool)component && component.SupportsPiece(placedPiece) && !component.HasSpace(freePlacementMask, placedPiece))
				{
					component.OccupySpaces(placedPiece.SpacesToOccupy, placedPiece);
				}
			}
		}

		public bool SupportsPiece(BuildingPiece piece)
		{
			for (int i = 0; i < m_PieceOffsets.Count; i++)
			{
				if (m_PieceOffsets[i] != null && m_PieceOffsets[i].Piece != null && m_PieceOffsets[i].Piece.Name == piece.Name && !m_OccupiedSpaces.Contains(piece.NeededSpace))
				{
					return true;
				}
			}
			return false;
		}

		private void OnDrawGizmos()
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one * 0.15f);
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = matrix;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
			Gizmos.DrawSphere(base.transform.position, m_Radius);
		}
	}
}
