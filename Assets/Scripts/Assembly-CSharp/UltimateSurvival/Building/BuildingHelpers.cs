using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.Building
{
	[Serializable]
	public class BuildingHelpers
	{
		[SerializeField]
		private LayerMask m_BuildingPieceMask;

		[SerializeField]
		private LayerMask m_FreePlacementMask;

		[SerializeField]
		private int m_BuildRange;

		[Header("Preview Pulsing Effect")]
		[SerializeField]
		private bool m_UsePulseEffect = true;

		[SerializeField]
		private bool m_PulseWhenSnapped = true;

		[SerializeField]
		private float m_PulseEffectDuration = 2f;

		[SerializeField]
		private float m_PulseMin = 0.4f;

		[SerializeField]
		private float m_PulseMax = 0.9f;

		[SerializeField]
		private Transform buildingsParent;

		[SerializeField]
		private SceneSave SceneSave;

		private BuildingPiece m_CurrentPreviewPiece;

		private GameObject m_CurrentPreview;

		private Socket m_LastValidSocket;

		private bool m_HasSocket;

		private bool m_PlacementAllowed = true;

		private Color m_PreviewColor;

		private float m_RotationOffset;

		private Transform m_Transform;

		private AlphaPulse m_Pulse;

		private BuildingPiece m_CurrentPrefab;

		public BuildingPiece CurrentPreviewPiece
		{
			get
			{
				return m_CurrentPreviewPiece;
			}
		}

		public bool HasSocket
		{
			get
			{
				return m_HasSocket;
			}
			set
			{
				m_HasSocket = value;
			}
		}

		public bool PlacementAllowed
		{
			get
			{
				return m_PlacementAllowed;
			}
		}

		public Color PreviewColor
		{
			get
			{
				return m_PreviewColor;
			}
			set
			{
				m_PreviewColor = value;
			}
		}

		public float RotationOffset
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

		public void Initialize(Transform t, PlayerEventHandler pl, AudioSource aS)
		{
			m_Transform = t;
			m_Pulse = new AlphaPulse(m_PreviewColor, m_PulseMin, m_PulseMax);
		}

		public void ManagePreview()
		{
			ManageCollision();
			if (m_UsePulseEffect)
			{
				ApplyPulse();
			}
			List<Renderer> renderers = m_CurrentPreviewPiece.Renderers;
			for (int i = 0; i < renderers.Count; i++)
			{
				Material[] materials = renderers[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].color = m_PreviewColor;
				}
				renderers[i].materials = materials;
			}
		}

		private void ManageCollision()
		{
			bool flag = m_CurrentPreviewPiece.IsBlockedByTerrain();
			if (!flag)
			{
				Collider[] array = Physics.OverlapBox(m_CurrentPreviewPiece.Bounds.center, m_CurrentPreviewPiece.Bounds.extents, m_CurrentPreviewPiece.transform.rotation, m_FreePlacementMask, QueryTriggerInteraction.Ignore);
				for (int i = 0; i < array.Length; i++)
				{
					if (m_CurrentPreviewPiece.HasCollider(array[i]))
					{
						continue;
					}
					TerrainCollider terrainCollider = array[i] as TerrainCollider;
					if (terrainCollider == null)
					{
						BuildingPiece component = array[i].GetComponent<BuildingPiece>();
						if (!component || !m_HasSocket || !(component.Building == m_LastValidSocket.Piece.Building))
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (m_HasSocket)
			{
				m_PlacementAllowed = !flag;
			}
			else
			{
				m_PlacementAllowed = !m_CurrentPreviewPiece.RequiresSockets && !flag;
			}
			UpdatePreviewColor();
		}

		private void UpdatePreviewColor()
		{
			Color previewColor = ((!m_PlacementAllowed) ? new Color(1f, 0f, 0f, m_PreviewColor.a) : new Color(0f, 1f, 0f, m_PreviewColor.a));
			m_PreviewColor = previewColor;
		}

		private void ApplyPulse()
		{
			if (!m_PulseWhenSnapped && m_HasSocket)
			{
				m_PreviewColor.a = 1f;
				return;
			}
			m_Pulse.StartPulse(m_PulseEffectDuration);
			m_PreviewColor.a = m_Pulse.UpdatePulse();
		}

		public void LookForSnaps()
		{
			m_CurrentPreview.gameObject.SetActive(GameController.LocalPlayer.CanShowObjectPreview.Get());
			Collider[] array = Physics.OverlapSphere(m_Transform.position, m_BuildRange, m_BuildingPieceMask, QueryTriggerInteraction.Ignore);
			if (array.Length > 0)
			{
				HandleSnapPreview(array);
			}
			else if (!RaycastAndPlace())
			{
				HandleFreePreview();
			}
		}

		private void HandleFreePreview()
		{
			Transform transform = ((m_CurrentPreviewPiece.OutOfGroundHeight != 0f) ? GameController.WorldCamera.transform : m_Transform);
			Vector3 position = transform.position + transform.forward * m_BuildRange;
			if (m_CurrentPreviewPiece.OutOfGroundHeight == 0f)
			{
				Vector3 origin = m_CurrentPreview.transform.position + new Vector3(0f, 0.25f, 0f);
				RaycastHit hitInfo;
				if (Physics.Raycast(origin, Vector3.down, out hitInfo, 1f, m_FreePlacementMask, QueryTriggerInteraction.Ignore))
				{
					position.y = hitInfo.point.y;
				}
			}
			else
			{
				float min = ((!m_CurrentPreviewPiece.AllowUnderTerrainMovement) ? 0f : (m_Transform.position.y - m_CurrentPreviewPiece.OutOfGroundHeight));
				position.y = Mathf.Clamp(position.y, min, m_Transform.position.y + m_CurrentPreviewPiece.OutOfGroundHeight);
			}
			m_CurrentPreview.transform.position = position;
			m_CurrentPreview.transform.rotation = m_Transform.rotation * m_CurrentPrefab.transform.localRotation * Quaternion.Euler(m_CurrentPreviewPiece.RotationAxis * m_RotationOffset);
			m_LastValidSocket = null;
			m_HasSocket = false;
		}

		private void HandleSnapPreview(Collider[] buildingPieces)
		{
			Camera main = Camera.main;
			Ray ray = main.ViewportPointToRay(Vector3.one * 0.5f);
			float num = float.PositiveInfinity;
			Socket socket = null;
			for (int i = 0; i < buildingPieces.Length; i++)
			{
				BuildingPiece component = buildingPieces[i].GetComponent<BuildingPiece>();
				if (component == null || component.Sockets.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < component.Sockets.Length; j++)
				{
					Socket socket2 = component.Sockets[j];
					if (socket2.SupportsPiece(m_CurrentPreviewPiece) && (socket2.transform.position - m_Transform.position).sqrMagnitude < Mathf.Pow(m_BuildRange, 2f))
					{
						float num2 = Vector3.Angle(ray.direction, socket2.transform.position - ray.origin);
						if (num2 < num && num2 < 35f)
						{
							num = num2;
							socket = socket2;
						}
					}
				}
			}
			Socket.PieceOffset offset;
			if (socket != null && socket.GetPieceOffsetByName(m_CurrentPrefab.Name, out offset))
			{
				m_CurrentPreview.transform.position = socket.transform.position + socket.transform.TransformVector(offset.PositionOffset);
				m_CurrentPreview.transform.rotation = socket.transform.rotation * offset.RotationOffset;
				m_LastValidSocket = socket;
				m_HasSocket = true;
			}
			else if (!RaycastAndPlace())
			{
				HandleFreePreview();
			}
		}

		private bool RaycastAndPlace()
		{
			Camera main = Camera.main;
			Ray ray = main.ViewportPointToRay(Vector3.one * 0.5f);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, m_BuildRange, m_FreePlacementMask, QueryTriggerInteraction.Ignore))
			{
				m_CurrentPreview.transform.position = hitInfo.point;
				m_CurrentPreview.transform.rotation = m_Transform.rotation * m_CurrentPrefab.transform.localRotation * Quaternion.Euler(m_CurrentPreviewPiece.RotationAxis * m_RotationOffset);
				return true;
			}
			return false;
		}

		public void SpawnPreview(GameObject prefab)
		{
			m_CurrentPreview = UnityEngine.Object.Instantiate(prefab);
			m_CurrentPreview.transform.position = Vector3.one * 10000f;
			m_CurrentPreviewPiece = m_CurrentPreview.GetComponent<BuildingPiece>();
			m_CurrentPreviewPiece.SetState(PieceState.Preview);
			m_CurrentPrefab = prefab.GetComponent<BuildingPiece>();
		}

		public void PlacePiece()
		{
			if (m_CurrentPreview == null)
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_CurrentPrefab.gameObject, m_CurrentPreview.transform.position, m_CurrentPreview.transform.rotation);
			gameObject.transform.SetParent(null);
			BuildingPiece component = gameObject.GetComponent<BuildingPiece>();
			if ((bool)m_LastValidSocket && m_LastValidSocket.Piece.Building != null)
			{
				component.transform.SetParent(buildingsParent, true);
				component.AttachedOn = m_LastValidSocket.Piece;
				component.SetState(PieceState.Placed);
				m_LastValidSocket.OccupyNeighbours(m_FreePlacementMask, m_BuildingPieceMask, component);
				component.Building = m_LastValidSocket.Piece.Building;
				component.Building.AddPiece(component);
			}
			else
			{
				component.transform.SetParent(buildingsParent, true);
				component.Building = buildingsParent.GetComponent<BuildingHolder>();
				component.Building.AddPiece(component);
				Collider collider;
				if (component.HasSupport(out collider))
				{
					BuildingPiece component2 = collider.GetComponent<BuildingPiece>();
					if (component2 != null)
					{
						component.AttachedOn = component2;
					}
				}
				component.SetState(PieceState.Placed);
			}
			m_RotationOffset = 0f;
			if ((bool)component.PlacementFX)
			{
				UnityEngine.Object.Instantiate(component.PlacementFX, component.transform.position, component.transform.rotation);
			}
			m_LastValidSocket = null;
			m_HasSocket = false;
			SceneSave.SaveObjectPlacing();
		}

		public BuildingPiece PlacePiece(GameObject savedPiece, Vector3 pos, Quaternion rot)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(savedPiece, pos, rot);
			gameObject.transform.SetParent(null);
			BuildingPiece component = gameObject.GetComponent<BuildingPiece>();
			if ((bool)m_LastValidSocket && m_LastValidSocket.Piece.Building != null)
			{
				component.transform.SetParent(buildingsParent, true);
				component.AttachedOn = m_LastValidSocket.Piece;
				component.SetState(PieceState.Placed);
				m_LastValidSocket.OccupyNeighbours(m_FreePlacementMask, m_BuildingPieceMask, component);
				component.Building = m_LastValidSocket.Piece.Building;
				component.Building.AddPiece(component);
			}
			else
			{
				component.transform.SetParent(buildingsParent, true);
				component.Building = buildingsParent.GetComponent<BuildingHolder>();
				component.Building.AddPiece(component);
				Collider collider;
				if (component.HasSupport(out collider))
				{
					BuildingPiece component2 = collider.GetComponent<BuildingPiece>();
					if (component2 != null)
					{
						component.AttachedOn = component2;
					}
				}
				component.SetState(PieceState.Placed);
			}
			m_RotationOffset = 0f;
			m_LastValidSocket = null;
			m_HasSocket = false;
			return component;
		}

		private bool IntersectsSocket(Ray ray, Socket socket)
		{
			Vector3 vector = socket.transform.position - ray.origin;
			float num = Vector3.Dot(vector, ray.direction);
			if (num < 0f)
			{
				return false;
			}
			float num2 = Vector3.Dot(vector, vector) - num * num;
			if (num2 > socket.Radius * socket.Radius)
			{
				return false;
			}
			return true;
		}

		public void ClearPreview()
		{
			if (m_CurrentPreview != null)
			{
				UnityEngine.Object.Destroy(m_CurrentPreview.gameObject);
				m_CurrentPreview = null;
				m_CurrentPreviewPiece = null;
			}
		}

		public bool PreviewExists()
		{
			if ((bool)m_CurrentPreview)
			{
				return true;
			}
			return false;
		}
	}
}
