using UltimateSurvival.GUISystem;
using UnityEngine;
using Mirror;

namespace UltimateSurvival
{
    public class ItemPickup : NetworkBehaviour
    {
        public enum PickupMethod
        {
            WalkOver = 0,
            OnInteract = 1
        }

        [SerializeField]
        private PickupMethod m_PickupMethod;

        [SerializeField]
        private string m_DefaultItem;

        [SerializeField]
        private int m_DefaultAmount = 1;

        [SerializeField]
        private AudioClip m_OnDestroySound;

        [SerializeField]
        private float m_OnDestroyVolume = 0.5f;

        public SavableItem ItemToAdd { get; set; }

        private void Start()
        {
            // Инициализация предмета
            ItemDatabase database = null;

            // Пытаемся найти базу данных
            if (InventoryController.Instance != null)
            {
                database = InventoryController.Instance.Database;
            }
            else
            {
                // Ищем любой InventoryController в сцене
                InventoryController[] controllers = FindObjectsOfType<InventoryController>();
                foreach (var controller in controllers)
                {
                    if (controller.Database != null)
                    {
                        database = controller.Database;
                        break;
                    }
                }
            }

            if (database != null && !string.IsNullOrEmpty(m_DefaultItem))
            {
                ItemData itemData;
                if (database.FindItemByName(m_DefaultItem, out itemData))
                {
                    ItemToAdd = new SavableItem(itemData, m_DefaultAmount);
                }
            }
        }

        // Метод для взаимодействия (вызывается из InteractableObject)
        public void OnPickup(PlayerEventHandler player)
        {
            if (m_PickupMethod == PickupMethod.WalkOver || ItemToAdd == null)
            {
                return;
            }

            // Если это локальный игрок
            InventoryController inventory = player.GetComponent<InventoryController>();
            if (inventory != null && inventory.isLocalPlayer)
            {
                inventory.Client_PickupItem(gameObject);
            }
        }

        // Метод для автоподбора при ходьбе
        private void OnTriggerEnter(Collider other)
        {
            if (m_PickupMethod != PickupMethod.WalkOver || ItemToAdd == null) return;

            PlayerEventHandler player = other.GetComponent<PlayerEventHandler>();
            if (player != null)
            {
                InventoryController inventory = player.GetComponent<InventoryController>();
                if (inventory != null && inventory.isLocalPlayer)
                {
                    inventory.Client_PickupItem(gameObject);
                }
            }
        }

        // Сетевой метод для воспроизведения звука
        [ClientRpc]
        public void RpcPlayPickupSound()
        {
            if (m_OnDestroySound != null)
            {
                AudioSource.PlayClipAtPoint(m_OnDestroySound, transform.position, m_OnDestroyVolume);
            }
        }

        // Метод для настройки предмета в сети (вызывается из InventoryController)
        public void SetupForNetwork(SavableItem item, int amount)
        {
            ItemToAdd = item;
        }

        // Для обратной совместимости с одиночной игрой
        private void TryPickupLocal(PlayerEventHandler player)
        {
            if (ItemToAdd == null) return;

            ItemContainer container = GUIController.Instance.GetContainer("Inventory");
            if (container != null && container.TryAddItem(ItemToAdd))
            {
                if (m_OnDestroySound != null)
                {
                    AudioSource.PlayClipAtPoint(m_OnDestroySound, transform.position, m_OnDestroyVolume);
                }

                if (MessageDisplayer.Instance != null)
                {
                    MessageDisplayer.Instance.PushMessage(string.Format("Picked up <color=yellow>{0}</color> x {1}", ItemToAdd.Name, ItemToAdd.CurrentInStack));
                }

                Destroy(gameObject);
            }
        }
    }
}