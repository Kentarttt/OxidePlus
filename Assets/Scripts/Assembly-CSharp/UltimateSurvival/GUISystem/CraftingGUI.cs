using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class CraftingGUI : MonoBehaviour
	{
		public Window window;

		public Button closeButton;

		public GameObject inventoryButton;

		public GameObject controlButtons;

		public RecipeInspector recipeInspector;

		public MouseLook mouseLook;

		private bool active;

		public void Show()
		{
			active = true;
			if (!MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				MonoSingleton<InventoryController>.Instance.SetState.Try(ET.InventoryState.Closed);
			}
			window.Open();
			mouseLook.lockState = true;
			closeButton.onClick.AddListener(delegate
			{
				Close();
			});
			closeButton.gameObject.SetActive(true);
			controlButtons.SetActive(false);
			inventoryButton.SetActive(true);
			recipeInspector.ShowFirstRecipe();
		}

		public void Close()
		{
			if (active)
			{
				window.Close();
				recipeInspector.Close();
				mouseLook.lockState = false;
				closeButton.onClick.RemoveAllListeners();
				closeButton.gameObject.SetActive(false);
				controlButtons.SetActive(true);
				inventoryButton.SetActive(false);
				active = false;
			}
		}
	}
}
