using UltimateSurvival.GUISystem;
using UnityEngine;

namespace UltimateSurvival.Building
{
	public class PlayerBuilder : MonoBehaviour
	{
		[SerializeField]
		private BuildingHelpers m_BuildingHelpers;

		[SerializeField]
		[Tooltip("How fast the player can rotate buildables.")]
		private float m_RotationSpeed = 120f;

		[SerializeField]
		private AudioSource m_AudioSource;

		[SerializeField]
		[Tooltip("A shake that will be played when the player places an object.")]
		private GenericShake m_PlaceShake;

		private float m_NextTimeCanPlayAudio;

		private PlayerEventHandler m_Player;

		private ItemContainer m_InventoryContainer;

		private BuildingPiece m_SelectedPiece;

		public BuildingHelpers BuildingHelpers
		{
			get
			{
				return m_BuildingHelpers;
			}
		}

		private void Start()
		{
			m_Player = GameController.LocalPlayer;
			m_BuildingHelpers.Initialize(base.transform, m_Player, m_AudioSource);
			m_Player.PlaceObject.SetTryer(Try_Place);
			m_Player.PlaceObject.AddListener(m_BuildingHelpers.PlacePiece);
			m_Player.RotateObject.SetTryer(Try_RotateObject);
			m_Player.EquippedItem.AddChangeListener(OnChanged_EquippedItem);
			m_Player.SelectedBuildable.AddChangeListener(OnChanged_SelectedBuildable);
			m_InventoryContainer = MonoSingleton<GUIController>.Instance.GetContainer("Inventory");
		}

		private void OnChanged_EquippedItem()
		{
			SavableItem savableItem = m_Player.EquippedItem.Get();
			if (savableItem != null && savableItem.HasProperty("Is Placeable"))
			{
				m_Player.SelectedBuildable.Set(savableItem.ItemData.WorldObject.GetComponent<BuildingPiece>());
				GameController.LocalPlayer.CanShowObjectPreview.Set(true);
			}
			if (savableItem != null && savableItem.HasProperty("Allows Building"))
			{
				m_Player.SelectedBuildable.Set(m_SelectedPiece);
			}
			UpdatePreview();
		}

		private void OnChanged_SelectedBuildable()
		{
			if (m_Player.EquippedItem.Get() != null && m_Player.EquippedItem.Get().HasProperty("Allows Building"))
			{
				m_SelectedPiece = m_Player.SelectedBuildable.Get();
			}
			UpdatePreview();
		}

		private void UpdatePreview()
		{
			if (m_BuildingHelpers.PreviewExists())
			{
				m_BuildingHelpers.ClearPreview();
			}
			BuildingPiece buildingPiece = m_Player.SelectedBuildable.Get();
			if (buildingPiece != null && m_Player.EquippedItem.Get() != null && (m_Player.EquippedItem.Get().HasProperty("Allows Building") || m_Player.EquippedItem.Get().HasProperty("Is Placeable")))
			{
				m_BuildingHelpers.SpawnPreview(buildingPiece.gameObject);
				m_BuildingHelpers.PreviewColor = m_BuildingHelpers.CurrentPreviewPiece.Renderers[0].material.color;
			}
			else
			{
				m_BuildingHelpers.ClearPreview();
			}
		}

		private bool Try_RotateObject(float rotationSign)
		{
			if (m_Player.ViewLocked.Is(false) && m_BuildingHelpers.PreviewExists())
			{
				m_BuildingHelpers.RotationOffset += m_RotationSpeed * rotationSign;
				return true;
			}
			return false;
		}

		private bool Try_Place()
		{
			if (!m_BuildingHelpers.PreviewExists() || !m_BuildingHelpers.PlacementAllowed)
			{
				return false;
			}
			if (m_Player.EquippedItem.Get().HasProperty("Is Placeable"))
			{
				m_BuildingHelpers.PlacePiece();
				m_BuildingHelpers.CurrentPreviewPiece.BuildAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
				m_PlaceShake.Shake(1f);
				SavableItem savableItem = m_Player.EquippedItem.Get();
				if (savableItem.CurrentInStack > 1)
				{
					savableItem.CurrentInStack--;
				}
				else
				{
					m_Player.DestroyEquippedItem.Try();
				}
				return true;
			}
			if (HasRequiredItems())
			{
				m_BuildingHelpers.CurrentPreviewPiece.BuildAudio.Play(ItemSelectionMethod.RandomlyButExcludeLast, m_AudioSource);
				m_PlaceShake.Shake(1f);
				for (int i = 0; i < m_BuildingHelpers.CurrentPreviewPiece.RequiredItems.Length; i++)
				{
					RequiredItem requiredItem = m_BuildingHelpers.CurrentPreviewPiece.RequiredItems[i];
					m_InventoryContainer.RemoveItems(requiredItem.Name, requiredItem.Amount);
				}
				return true;
			}
			string text = "You don't have all the required materials: \n";
			for (int j = 0; j < m_BuildingHelpers.CurrentPreviewPiece.RequiredItems.Length; j++)
			{
				RequiredItem requiredItem2 = m_BuildingHelpers.CurrentPreviewPiece.RequiredItems[j];
				text += string.Format("<color=yellow>{0}</color> x {1}, ", requiredItem2.Name, requiredItem2.Amount);
			}
			MonoSingleton<MessageDisplayer>.Instance.PushMessage(text, default(Color), 48);
			return false;
		}

		private bool HasRequiredItems()
		{
			for (int i = 0; i < m_BuildingHelpers.CurrentPreviewPiece.RequiredItems.Length; i++)
			{
				RequiredItem requiredItem = m_BuildingHelpers.CurrentPreviewPiece.RequiredItems[i];
				if (m_InventoryContainer.GetItemCount(requiredItem.Name) < requiredItem.Amount)
				{
					return false;
				}
			}
			return true;
		}

		private void Update()
		{
			m_BuildingHelpers.HasSocket = false;
			if (m_BuildingHelpers.PreviewExists())
			{
				m_BuildingHelpers.LookForSnaps();
			}
			if (m_BuildingHelpers.PreviewExists())
			{
				m_BuildingHelpers.ManagePreview();
			}
		}

		public bool CanPlace()
		{
			return m_BuildingHelpers.PlacementAllowed;
		}
	}
}
