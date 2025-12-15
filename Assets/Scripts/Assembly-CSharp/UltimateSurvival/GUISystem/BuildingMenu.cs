using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class BuildingMenu : GUIBehaviour
	{
		public BuildingCategory SelectedCategory;

		[SerializeField]
		private Window m_Window;

		[SerializeField]
		private Camera m_GUICamera;

		[SerializeField]
		private RectTransform m_SelectionHighlight;

		[SerializeField]
		private Text m_CategoryName;

		[Header("Audio")]
		[SerializeField]
		private SoundPlayer m_RefreshAudio;

		[SerializeField]
		private SoundPlayer m_SelectPieceAudio;

		[Header("Layout")]
		[SerializeField]
		private float m_Distance = 211.7f;

		[SerializeField]
		[Range(-90f, 90f)]
		private float m_Spacing;

		private BuildingCategory[] m_Categories;

		private BuildingCategory m_SelectedCategory;

		private int m_CategoryIndex;

		[SerializeField]
		private BuildingCategory m_FirstSelected;

		[SerializeField]
		private GameObject ControlButtons;

		[SerializeField]
		private GameObject BuildButtons;

		[SerializeField]
		private GameObject Hotbar;

		[SerializeField]
		private Button CloseButton;

		public RectTransform SelectionHighlight
		{
			get
			{
				return m_SelectionHighlight;
			}
		}

		public float Distance
		{
			get
			{
				return m_Distance;
			}
		}

		public float Spacing
		{
			get
			{
				return m_Spacing;
			}
		}

		private void Start()
		{
			base.Player.SelectBuildable.AddStartTryer(TryStart_SelectBuildable);
			base.Player.SelectBuildable.AddStopTryer(TryStop_SelectBuildable);
			MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnChanged_InventoryState);
		}

		private void OnChanged_InventoryState()
		{
			if (!MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				while (base.Player.SelectBuildable.Active)
				{
					base.Player.SelectBuildable.TryStop();
				}
			}
		}

		private bool TryStart_SelectBuildable()
		{
			if (!MonoSingleton<InventoryController>.Instance.IsClosed || !base.Player.EquippedItem.Get() || !base.Player.EquippedItem.Get().HasProperty("Allows Building"))
			{
				return false;
			}
			ControlButtons.SetActive(false);
			BuildButtons.SetActive(false);
			Hotbar.SetActive(false);
			CloseButton.onClick.AddListener(delegate
			{
				base.Player.SelectBuildable.TryStop();
			});
			CloseButton.gameObject.SetActive(true);
			m_Window.Open();
			return true;
		}

		private bool TryStop_SelectBuildable()
		{
			m_Window.Close();
			CloseButton.gameObject.SetActive(false);
			if (base.Player.CanShowObjectPreview.Is(false))
			{
				ControlButtons.SetActive(true);
				Hotbar.SetActive(true);
				CloseButton.onClick.RemoveAllListeners();
			}
			else
			{
				BuildButtons.SetActive(true);
			}
			return true;
		}

		public void SelectCategory(BuildingCategory category)
		{
			if ((bool)m_SelectedCategory)
			{
				m_SelectedCategory.ShowPieces(false);
			}
			m_SelectedCategory = category;
			if (category.CategoryIndex > 0)
			{
				m_SelectedCategory.ShowPieces(true);
			}
			m_CategoryIndex = category.CategoryIndex;
			float num = Spacing * (float)m_CategoryIndex;
			m_SelectionHighlight.localPosition = Quaternion.Euler(Vector3.back * num) * Vector3.up * Distance;
			m_SelectionHighlight.localRotation = Quaternion.Euler(Vector3.back * num);
			m_RefreshAudio.Play2D();
			if ((bool)m_CategoryName)
			{
				m_CategoryName.text = category.CategoryName;
			}
		}

		public void SelectPiece(BuildingPiece piece)
		{
			base.Player.SelectedBuildable.Set(piece.BuildableObject);
			base.Player.CanShowObjectPreview.Set(true);
			base.Player.SelectBuildable.TryStop();
		}
	}
}
