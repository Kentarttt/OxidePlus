using UltimateSurvival.InputSystem;
using UnityEngine;

namespace UltimateSurvival
{
	public class GameController : MonoBehaviour
	{
		[SerializeField]
		private SurfaceDatabase m_SurfaceDatabase;

		[SerializeField]
		private ItemDatabase m_ItemDatabase;

		private static InputManager m_InputManager;

		private static PlayerEventHandler m_Player;

		private static SceneSave m_SceneSave;

		public Camera mainCamera;

		public static PlayerEventHandler LocalPlayer
		{
			get
			{
				if (m_Player == null)
				{
					m_Player = Object.FindObjectOfType<PlayerEventHandler>();
				}
				return m_Player;
			}
		}

		public static InputManager InputManager
		{
			get
			{
				if (m_InputManager == null)
				{
					m_InputManager = Object.FindObjectOfType<InputManager>();
				}
				return m_InputManager;
			}
		}

		public static float NormalizedTime { get; set; }

		public static AudioUtils Audio { get; private set; }

		public static Camera WorldCamera { get; private set; }

		public static SurfaceDatabase SurfaceDatabase { get; private set; }

		public static ItemDatabase ItemDatabase { get; private set; }

		public static TreeManager TerrainHelpers { get; private set; }

		public static SceneSave SceneSave
		{
			get
			{
				if (m_SceneSave == null)
				{
					m_SceneSave = Object.FindObjectOfType<SceneSave>();
				}
				return m_SceneSave;
			}
		}

		private void Awake()
		{
			Audio = GetComponentInChildren<AudioUtils>();
			WorldCamera = LocalPlayer.transform.FindDeepChild("World Camera").GetComponent<Camera>();
			SurfaceDatabase = m_SurfaceDatabase;
			ItemDatabase = m_ItemDatabase;
			TerrainHelpers = GetComponent<TreeManager>();
			if (PlayerPrefs.HasKey("draw_distance"))
			{
				int num = PlayerPrefs.GetInt("draw_distance") * 100;
				Terrain.activeTerrain.treeDistance = num;
				Terrain.activeTerrain.basemapDistance = num;
				mainCamera.farClipPlane = num;
			}
		}
	}
}
