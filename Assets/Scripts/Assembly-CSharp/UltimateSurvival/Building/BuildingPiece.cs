using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival.Building
{
	public class BuildingPiece : MonoBehaviour
	{
		[SerializeField]
		private string m_PieceName;

		[SerializeField]
		private RequiredItem[] m_RequiredItems;

		[SerializeField]
		private Vector3 m_RotationAxis = Vector3.forward;

		[Header("Setup")]
		[SerializeField]
		private bool m_ShowBounds;

		[SerializeField]
		private Bounds m_Bounds;

		[SerializeField]
		[Tooltip("If left empty, it will automatically get populated with the first MeshFilter found.")]
		private MeshFilter m_MainMesh;

		[SerializeField]
		private List<Renderer> m_IgnoredRenderers;

		[SerializeField]
		private List<Collider> m_IgnoredColliders;

		[Header("Placing")]
		[SerializeField]
		private BuildingSpace m_NeededSpace;

		[SerializeField]
		private BuildingSpace[] m_SpacesToOccupy;

		[SerializeField]
		private bool m_RequiresSockets;

		[SerializeField]
		private float m_OutOfGroundHeight;

		[SerializeField]
		private bool m_AllowUnderTerrainMovement;

		[Header("Stability")]
		[SerializeField]
		private bool m_CheckStability = true;

		[Space]
		[SerializeField]
		private LayerMask m_StabilityCheckMask;

		[SerializeField]
		private bool m_ShowStabilityBox;

		[SerializeField]
		private Bounds m_StabilityBox;

		[Header("Terrain Protection")]
		[SerializeField]
		private bool m_EnableTP;

		[SerializeField]
		private bool m_ShowTP;

		[SerializeField]
		private Bounds m_TPBox;

		[Header("Sound And Effects")]
		[SerializeField]
		private SoundPlayer m_BuildAudio;

		[SerializeField]
		private GameObject m_PlacementFX;

		[Header("Preview")]
		[SerializeField]
		private Material m_PreviewMat;

		private Dictionary<Renderer, Material[]> m_InitialMaterials = new Dictionary<Renderer, Material[]>();

		private Socket[] m_Sockets = new Socket[0];

		private PieceState m_State;

		private List<Collider> m_Colliders = new List<Collider>();

		private List<Renderer> m_Renderers = new List<Renderer>();

		private bool m_Initialized;

		public PieceState State
		{
			get
			{
				return m_State;
			}
		}

		public BuildingHolder Building { get; set; }

		public BuildingPiece AttachedOn { get; set; }

		public string Name
		{
			get
			{
				return m_PieceName;
			}
		}

		public RequiredItem[] RequiredItems
		{
			get
			{
				return m_RequiredItems;
			}
		}

		public Vector3 RotationAxis
		{
			get
			{
				return m_RotationAxis;
			}
		}

		public BuildingSpace NeededSpace
		{
			get
			{
				return m_NeededSpace;
			}
		}

		public BuildingSpace[] SpacesToOccupy
		{
			get
			{
				return m_SpacesToOccupy;
			}
		}

		public float OutOfGroundHeight
		{
			get
			{
				return m_OutOfGroundHeight;
			}
		}

		public bool AllowUnderTerrainMovement
		{
			get
			{
				return m_AllowUnderTerrainMovement;
			}
		}

		public bool RequiresSockets
		{
			get
			{
				return m_RequiresSockets;
			}
		}

		public SoundPlayer BuildAudio
		{
			get
			{
				return m_BuildAudio;
			}
		}

		public GameObject PlacementFX
		{
			get
			{
				return m_PlacementFX;
			}
		}

		public Bounds Bounds
		{
			get
			{
				return new Bounds(base.transform.position + base.transform.TransformVector(m_Bounds.center), m_Bounds.size);
			}
		}

		public List<Renderer> Renderers
		{
			get
			{
				return m_Renderers;
			}
		}

		public MeshFilter MainMesh
		{
			get
			{
				return m_MainMesh;
			}
		}

		public Socket[] Sockets
		{
			get
			{
				return m_Sockets;
			}
		}

		private void Awake()
		{
			if (m_MainMesh == null)
			{
				m_MainMesh = GetComponentInChildren<MeshFilter>();
			}
			GetComponentsInChildren(m_Renderers);
			m_Renderers.RemoveAll((Renderer r) => m_IgnoredRenderers.Contains(r));
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				m_InitialMaterials.Add(m_Renderers[i], m_Renderers[i].sharedMaterials);
			}
			GetComponentsInChildren(m_Colliders);
			m_Colliders.RemoveAll((Collider col) => m_IgnoredColliders.Contains(col));
			for (int j = 0; j < m_Colliders.Count; j++)
			{
				for (int k = 0; k < m_Colliders.Count; k++)
				{
					if (m_Colliders[j] != m_Colliders[k])
					{
						Physics.IgnoreCollision(m_Colliders[j], m_Colliders[k]);
					}
				}
			}
			m_Sockets = GetComponentsInChildren<Socket>();
			m_Initialized = true;
		}

		public void SetState(PieceState state)
		{
			if (!m_Initialized)
			{
				Awake();
			}
			switch (state)
			{
			case PieceState.Preview:
			{
				SetMaterials(m_PreviewMat);
				foreach (Collider collider in m_Colliders)
				{
					if ((bool)collider)
					{
						collider.enabled = false;
					}
					else
					{
						Debug.LogError("A collider was found null in the collider list!", this);
					}
				}
				Socket[] sockets2 = m_Sockets;
				foreach (Socket socket2 in sockets2)
				{
					socket2.gameObject.SetActive(false);
				}
				break;
			}
			case PieceState.Placed:
			{
				SetMaterials(m_InitialMaterials);
				foreach (Collider collider2 in m_Colliders)
				{
					collider2.enabled = true;
				}
				Socket[] sockets = m_Sockets;
				foreach (Socket socket in sockets)
				{
					socket.gameObject.SetActive(true);
				}
				if (m_CheckStability && AttachedOn != null)
				{
					EntityEventHandler component = AttachedOn.GetComponent<EntityEventHandler>();
					if (component != null)
					{
						component.Death.AddListener(On_SocketDeath);
					}
				}
				break;
			}
			}
			m_State = state;
		}

		private void On_SocketDeath()
		{
			if (base.gameObject != null)
			{
				EntityEventHandler component = GetComponent<EntityEventHandler>();
				component.ChangeHealth.Try(new HealthEventData(float.NegativeInfinity));
			}
		}

		private void OnDestroy()
		{
			if (AttachedOn != null)
			{
				AttachedOn.GetComponent<EntityEventHandler>().Death.RemoveListener(On_SocketDeath);
			}
			if ((bool)GameController.SceneSave)
			{
				GameController.SceneSave.SaveObjectPlacing();
			}
		}

		private void SetMaterials(Material material)
		{
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				Material[] materials = m_Renderers[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j] = material;
				}
				m_Renderers[i].materials = materials;
			}
		}

		private void SetMaterials(Dictionary<Renderer, Material[]> materials)
		{
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				Material[] materials2 = m_Renderers[i].materials;
				for (int j = 0; j < materials2.Length; j++)
				{
					materials2[j] = materials[m_Renderers[i]][j];
				}
				m_Renderers[i].materials = materials2;
			}
		}

		public bool IsBlockedByTerrain()
		{
			if (!m_EnableTP)
			{
				return false;
			}
			Collider[] array = Physics.OverlapBox(base.transform.position + base.transform.TransformVector(m_TPBox.center), m_TPBox.extents, base.transform.rotation, -1, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] as TerrainCollider != null)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasCollider(Collider col)
		{
			for (int i = 0; i < m_Colliders.Count; i++)
			{
				if (m_Colliders[i] == col)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasSupport(out Collider collider)
		{
			Collider[] array = Physics.OverlapBox(base.transform.position + base.transform.TransformVector(m_StabilityBox.center), m_StabilityBox.extents, base.transform.rotation, m_StabilityCheckMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].isTrigger && !HasCollider(array[i]))
				{
					collider = array[i];
					return true;
				}
			}
			collider = null;
			return false;
		}

		private void OnDrawGizmosSelected()
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.color = Color.blue;
			if (m_ShowBounds)
			{
				Gizmos.matrix = Matrix4x4.TRS(base.transform.position + base.transform.TransformVector(m_Bounds.center), base.transform.rotation, m_Bounds.size);
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
			Gizmos.color = Color.yellow;
			if (m_ShowTP)
			{
				Gizmos.matrix = Matrix4x4.TRS(base.transform.position + base.transform.TransformVector(m_TPBox.center), base.transform.rotation, m_TPBox.size);
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
			Gizmos.color = Color.red;
			if (m_ShowStabilityBox)
			{
				Gizmos.matrix = Matrix4x4.TRS(base.transform.position + base.transform.TransformVector(m_StabilityBox.center), base.transform.rotation, m_StabilityBox.size);
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
			Gizmos.matrix = matrix;
		}
	}
}
