using System.Collections.Generic;
using UltimateSurvival.GUISystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

namespace UltimateSurvival
{
    public class InventoryController : NetworkBehaviour
    {
        public Value<ET.InventoryState> State = new Value<ET.InventoryState>(ET.InventoryState.Closed);
        public Attempt<ET.InventoryState> SetState = new Attempt<ET.InventoryState>();

        public Attempt<SmeltingStation> OpenFurnace = new Attempt<SmeltingStation>();
        public Attempt<SmeltingStation> OpenCampfire = new Attempt<SmeltingStation>();
        public Attempt<LootObject> OpenLootContainer = new Attempt<LootObject>();
        public Attempt<Anvil> OpenAnvil = new Attempt<Anvil>();
        public Attempt<CraftData> CraftItem = new Attempt<CraftData>();

        // Для обратной совместимости со старым кодом
        public Message<ItemHolder> EquipmentChanged = new Message<ItemHolder>();

        [SerializeField]
        [Tooltip("The inventory cannot function without this, as some operations, like ADD, LOAD require a database.")]
        private ItemDatabase m_ItemDatabase;

        [Header("Item Collections")]
        [SerializeField]
        [Range(1, 48)]
        private int m_InventorySize = 24;

        [SerializeField]
        [Range(1, 12)]
        private int m_HotbarSize = 6;

        [SerializeField]
        [Reorderable]
        private ReorderableStringList m_EquipmentList;

        [Header("Item Drop")]
        [SerializeField]
        private Vector3 m_DropOffset = new Vector3(0f, 0f, 0.8f);

        [SerializeField]
        private float m_DropAngularFactor = 150f;

        [SerializeField]
        private float m_DropSpeed = 8f;

        private PlayerEventHandler m_Player;
        private ItemContainer[] m_AllCollections;

        [SyncVar(hook = nameof(OnInventoryChanged))]
        private string m_SyncInventoryData;

        [SyncVar(hook = nameof(OnHotbarChanged))]
        private string m_SyncHotbarData;

        [SyncVar(hook = nameof(OnEquipmentChangedHook))]
        private string m_SyncEquipmentData;

        private List<ItemHolder> m_InventoryCollection;
        private List<ItemHolder> m_HotbarCollection;
        private List<ItemHolder> m_EquipmentHolders;

        [Space(10f)]
        public PlayerStats playerStats;
        public Button closeButton;
        public GameObject craftButton;
        public GameObject controlButtons;

        public bool IsClosed => State.Is(ET.InventoryState.Closed);

        public ItemDatabase Database => m_ItemDatabase;

        #region Публичные методы для обратной совместимости

        // Старые методы для обратной совместимости
        public bool AddItemToCollection(int itemID, int amount, string collection, out int added)
        {
            if (isServer)
            {
                return Server_AddItemToCollection(itemID, amount, collection, out added);
            }
            else if (isLocalPlayer)
            {
                // Для клиента - отправляем команду на сервер
                CmdAddItem(itemID, amount, collection);
                added = amount; // Предполагаем успех (реальное значение будет на сервере)
                return true;
            }
            added = 0;
            return false;
        }

        public bool AddItemToCollection(int itemID, int amount, int slotId, string collection, out int added)
        {
            if (isServer)
            {
                return Server_AddItemToCollection(itemID, amount, slotId, collection, out added);
            }
            else if (isLocalPlayer)
            {
                CmdAddItemToSlot(itemID, amount, slotId, collection);
                added = amount;
                return true;
            }
            added = 0;
            return false;
        }

        public bool AddItemToCollection(string itemName, int amount, string collection, out int added)
        {
            if (isServer)
            {
                ItemData itemData;
                if (m_ItemDatabase.FindItemByName(itemName, out itemData))
                {
                    return Server_AddItemToCollection(itemData.Id, amount, collection, out added);
                }
                added = 0;
                return false;
            }
            else if (isLocalPlayer)
            {
                CmdAddItemByName(itemName, amount, collection);
                added = amount;
                return true;
            }
            added = 0;
            return false;
        }

        public void RemoveItems(string itemName, int amount = 1)
        {
            if (isServer)
            {
                ItemContainer container = GUIController.Instance.GetContainer("Inventory");
                if (container != null)
                {
                    container.RemoveItems(itemName, amount);
                    SyncCollectionsToClients();
                }
            }
            else if (isLocalPlayer)
            {
                CmdRemoveItem(itemName, amount);
            }
        }

        // Перегрузка без параметра amount для обратной совместимости
        public void RemoveItems(string itemName)
        {
            RemoveItems(itemName, 1);
        }

        public bool TryRemoveItem(SavableItem item)
        {
            if (isServer)
            {
                for (int i = 0; i < m_AllCollections.Length; i++)
                {
                    if (m_AllCollections[i].TryRemoveItem(item))
                    {
                        SyncCollectionsToClients();
                        return true;
                    }
                }
                return false;
            }
            // Для клиента нужно найти предмет и отправить команду
            return false;
        }

        #endregion

        #region Сетевые команды

        [Command]
        public void CmdPickupItem(GameObject pickupObject)
        {
            if (!isServer || pickupObject == null) return;

            ItemPickup pickup = pickupObject.GetComponent<ItemPickup>();
            if (pickup != null && pickup.ItemToAdd != null)
            {
                int added;
                if (Server_AddItemToCollection(pickup.ItemToAdd.ItemData.Id, pickup.ItemToAdd.CurrentInStack, "Inventory", out added))
                {
                    // Воспроизводим звук на всех клиентах
                    pickup.RpcPlayPickupSound();

                    // Отправляем сообщение игроку
                    TargetShowPickupMessage(connectionToClient, pickup.ItemToAdd.Name, added);

                    // Уничтожаем предмет на всех клиентах
                    NetworkServer.Destroy(pickupObject);
                }
            }
        }

        [TargetRpc]
        private void TargetShowPickupMessage(NetworkConnection target, string itemName, int amount)
        {
            if (MessageDisplayer.Instance != null)
            {
                MessageDisplayer.Instance.PushMessage(string.Format("Picked up <color=yellow>{0}</color> x {1}", itemName, amount));
            }
        }

        [Command]
        public void CmdAddItem(int itemID, int amount, string collection)
        {
            int added;
            Server_AddItemToCollection(itemID, amount, collection, out added);
        }

        [Command]
        public void CmdAddItemToSlot(int itemID, int amount, int slotId, string collection)
        {
            int added;
            Server_AddItemToCollection(itemID, amount, slotId, collection, out added);
        }

        [Command]
        public void CmdAddItemByName(string itemName, int amount, string collection)
        {
            if (!isServer) return;

            ItemData itemData;
            if (m_ItemDatabase.FindItemByName(itemName, out itemData))
            {
                int added;
                Server_AddItemToCollection(itemData.Id, amount, collection, out added);
            }
        }

        [Command]
        public void CmdRemoveItem(string itemName, int amount)
        {
            if (!isServer) return;

            ItemContainer container = GUIController.Instance.GetContainer("Inventory");
            if (container != null)
            {
                container.RemoveItems(itemName, amount);
                SyncCollectionsToClients();
            }
        }

        // Перегрузка для удаления одного предмета
        [Command]
        public void CmdRemoveItemSingle(string itemName)
        {
            CmdRemoveItem(itemName, 1);
        }

        [Server]
        public bool Server_AddItemToCollection(int itemID, int amount, string collection, out int added)
        {
            added = 0;
            if (!isServer || !enabled) return false;

            for (int i = 0; i < m_AllCollections.Length; i++)
            {
                if (m_AllCollections[i].Name == collection)
                {
                    bool result = false;
                    ItemData itemData;

                    if (m_ItemDatabase.FindItemById(itemID, out itemData))
                    {
                        result = m_AllCollections[i].TryAddItem(itemData, amount, out added);
                        if (result)
                        {
                            SyncCollectionsToClients();
                        }
                    }
                    return result;
                }
            }

            Debug.LogWarningFormat(this, "No collection with the name '{0}' was found! No item added.", collection);
            return false;
        }

        [Server]
        public bool Server_AddItemToCollection(int itemID, int amount, int slotId, string collection, out int added)
        {
            added = 0;
            if (!isServer || !enabled) return false;

            for (int i = 0; i < m_AllCollections.Length; i++)
            {
                if (m_AllCollections[i].Name == collection)
                {
                    bool result = false;
                    ItemData itemData;

                    if (m_ItemDatabase.FindItemById(itemID, out itemData))
                    {
                        result = m_AllCollections[i].TryAddItem(itemData, amount, slotId, out added);
                        if (result)
                        {
                            SyncCollectionsToClients();
                        }
                    }
                    return result;
                }
            }

            Debug.LogWarningFormat(this, "No collection with the name '{0}' was found! No item added.", collection);
            return false;
        }

        #endregion

        public int GetItemCount(string name)
        {
            if (!isLocalPlayer) return 0;
            return GUIController.Instance.GetContainer("Inventory").GetItemCount(name);
        }

        [Command]
        public void CmdRemoveSpecificItem(int slotIndex, string collectionName)
        {
            if (!isServer) return;

            ItemContainer container = GUIController.Instance.GetContainer(collectionName);
            if (container != null && slotIndex >= 0 && slotIndex < container.Slots.Count)
            {
                Slot slot = container.Slots[slotIndex];
                if (slot.HasItem)
                {
                    slot.ItemHolder.SetItem(null);
                    SyncCollectionsToClients();
                }
            }
        }

        [Command]
        public void CmdDropItem(int slotIndex, string collectionName)
        {
            if (!isServer) return;

            ItemContainer container = GUIController.Instance.GetContainer(collectionName);
            if (container != null && slotIndex >= 0 && slotIndex < container.Slots.Count)
            {
                Slot slot = container.Slots[slotIndex];
                if (slot.HasItem)
                {
                    Try_DropItem(slot.CurrentItem, slot);
                    SyncCollectionsToClients();
                }
            }
        }

        [Server]
        public bool Try_DropItem(SavableItem item, Slot parentSlot = null)
        {
            if (item != null && item.ItemData.WorldObject != null && !item.ItemData.IsBuildable)
            {
                Transform playerTransform = m_Player.transform;
                GameObject droppedItem = Instantiate(
                    item.ItemData.WorldObject,
                    playerTransform.position + playerTransform.TransformVector(m_DropOffset),
                    Random.rotation
                );

                NetworkServer.Spawn(droppedItem);

                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.angularVelocity = Random.rotation.eulerAngles * m_DropAngularFactor;
                    rb.AddForce(playerTransform.forward * m_DropSpeed, ForceMode.VelocityChange);
                }

                ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
                if (pickup != null)
                {
                    pickup.ItemToAdd = item;
                    // Если есть метод SetupForNetwork - вызываем его через рефлексию
                    var pickupType = pickup.GetType();
                    var setupMethod = pickupType.GetMethod("SetupForNetwork");
                    if (setupMethod != null)
                    {
                        setupMethod.Invoke(pickup, new object[] { item, item.CurrentInStack });
                    }
                }

                if (parentSlot != null)
                {
                    parentSlot.ItemHolder.SetItem(null);
                }

                return true;
            }
            return false;
        }

        public List<ItemHolder> GetEquipmentHolders()
        {
            return m_EquipmentHolders;
        }

        private void Awake()
        {
            if (!m_ItemDatabase)
            {
                Debug.LogError("No ItemDatabase specified, the inventory will be disabled!", this);
                enabled = false;
                return;
            }

            SetState.SetTryer(TryChange_State);

            if (GUIController.Instance != null)
            {
                m_AllCollections = GUIController.Instance.Containers;

                m_InventoryCollection = CreateListOfHolders(m_InventorySize);
                ItemContainer inventoryContainer = GUIController.Instance.GetContainer("Inventory");
                if (inventoryContainer != null)
                    inventoryContainer.Setup(m_InventoryCollection);

                m_HotbarCollection = CreateListOfHolders(m_HotbarSize);
                ItemContainer hotbarContainer = GUIController.Instance.GetContainer("Hotbar");
                if (hotbarContainer != null)
                    hotbarContainer.Setup(m_HotbarCollection);

                m_EquipmentHolders = CreateListOfHolders(m_EquipmentList.Count);
                for (int i = 0; i < m_EquipmentList.Count; i++)
                {
                    ItemContainer equipmentContainer = GUIController.Instance.GetContainer(m_EquipmentList[i]);
                    if (equipmentContainer != null)
                    {
                        equipmentContainer.Setup(new List<ItemHolder> { m_EquipmentHolders[i] });
                    }
                    else
                    {
                        Debug.LogErrorFormat(this, "No GUI collection with the name '{0}' was found!", m_EquipmentList[i]);
                    }
                }
            }

            m_Player = GetComponent<PlayerEventHandler>();
            if (m_Player != null)
            {
                m_Player.ChangeHealth.AddListener(OnChanged_PlayerHealth);
                m_Player.Death.AddListener(On_PlayerDeath);
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (isLocalPlayer)
            {
                CmdRequestInventoryData();
            }
        }

        [Command]
        private void CmdRequestInventoryData()
        {
            if (!isServer) return;

            TargetSyncInventoryData(
                connectionToClient,
                GetItemDataText(),
                GetHotbarDataText(),
                GetEquipDataText()
            );
        }

        [TargetRpc]
        private void TargetSyncInventoryData(NetworkConnection target, string inventoryData, string hotbarData, string equipData)
        {
            SetItemsFromText(inventoryData, "Inventory");
            SetItemsFromText(hotbarData, "Hotbar");
            SetEquipFromText(equipData);
        }

        [Server]
        private void SyncCollectionsToClients()
        {
            m_SyncInventoryData = GetItemDataText();
            m_SyncHotbarData = GetHotbarDataText();
            m_SyncEquipmentData = GetEquipDataText();
        }

        private void OnInventoryChanged(string oldData, string newData)
        {
            if (!isLocalPlayer) return;
            SetItemsFromText(newData, "Inventory");
        }

        private void OnHotbarChanged(string oldData, string newData)
        {
            if (!isLocalPlayer) return;
            SetItemsFromText(newData, "Hotbar");
        }

        private void OnEquipmentChangedHook(string oldData, string newData)
        {
            if (!isLocalPlayer) return;
            SetEquipFromText(newData);
        }

        private void OnChanged_PlayerHealth(HealthEventData data)
        {
            if (!isServer || data.Delta >= 0f) return;

            bool changed = false;

            for (int i = 0; i < m_EquipmentHolders.Count; i++)
            {
                if (m_EquipmentHolders[i].HasItem && m_EquipmentHolders[i].CurrentItem.HasProperty("Durability"))
                {
                    ItemProperty.Value propertyValue = m_EquipmentHolders[i].CurrentItem.GetPropertyValue("Durability");
                    ItemProperty.Float durability = propertyValue.Float;
                    durability.Current -= 1f;
                    propertyValue.SetValue(ItemProperty.Type.Float, durability);

                    if (durability.Current <= 0f)
                    {
                        m_EquipmentHolders[i].SetItem(null);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                SyncCollectionsToClients();
            }
        }

        [Server]
        private void On_PlayerDeath()
        {
            if (State.Get() != ET.InventoryState.Closed)
            {
                SetState.Try(ET.InventoryState.Closed);
            }

            DropItemsOnDeath();
            SyncCollectionsToClients();
        }

        [Server]
        private void DropItemsOnDeath()
        {
            DropItemsFromCollection("Inventory");
            DropItemsFromCollection("Hotbar");

            foreach (string equipment in m_EquipmentList)
            {
                DropItemsFromCollection(equipment);
            }
        }

        [Server]
        private void DropItemsFromCollection(string collection)
        {
            ItemContainer container = GUIController.Instance.GetContainer(collection);
            if (container == null) return;

            foreach (Slot slot in container.Slots)
            {
                if (slot.HasItem)
                {
                    if (slot.CurrentItem.ItemData.IsBuildable)
                    {
                        slot.ItemHolder.SetItem(null);
                    }
                    else
                    {
                        Try_DropItem(slot.CurrentItem, slot);
                    }
                }
            }
        }

        [Server]
        private void RemoveItemsFromCollection(string collection)
        {
            ItemContainer container = GUIController.Instance.GetContainer(collection);
            if (container == null) return;

            foreach (Slot slot in container.Slots)
            {
                if (slot.HasItem)
                {
                    slot.ItemHolder.SetItem(null);
                }
            }
        }

        private bool TryChange_State(ET.InventoryState state)
        {
            if (!isLocalPlayer) return false;

            State.Set(state);
            EventSystem.current.SetSelectedGameObject(null);

            if (state == ET.InventoryState.Closed)
            {
                controlButtons.SetActive(true);
                closeButton.onClick.RemoveAllListeners();
                closeButton.gameObject.SetActive(false);
                craftButton.SetActive(false);
            }
            else
            {
                closeButton.onClick.AddListener(delegate
                {
                    SetState.Try(ET.InventoryState.Closed);
                });
                closeButton.gameObject.SetActive(true);
                craftButton.SetActive(true);
                controlButtons.SetActive(false);
            }

            return true;
        }

        private List<ItemHolder> CreateListOfHolders(int size)
        {
            List<ItemHolder> holders = new List<ItemHolder>();
            for (int i = 0; i < size; i++)
            {
                holders.Add(new ItemHolder());
            }
            return holders;
        }

        public string GetItemDataText()
        {
            string itemIds = string.Empty;
            string stackSizes = string.Empty;
            string durabilities = string.Empty;

            foreach (ItemHolder item in m_InventoryCollection)
            {
                if (item.HasItem)
                {
                    itemIds += item.CurrentItem.ItemData.Id + ",";
                    stackSizes += item.CurrentItem.CurrentInStack + ",";
                    durabilities += (!item.CurrentItem.HasProperty("Durability")) ? "-1," : item.CurrentItem.GetPropertyValue("Durability").Float.Current + ",";
                }
                else
                {
                    itemIds += "-1,";
                    stackSizes += "-1,";
                    durabilities += "-1,";
                }
            }

            return itemIds + "|" + stackSizes + "|" + durabilities;
        }

        public string GetHotbarDataText()
        {
            string itemIds = string.Empty;
            string stackSizes = string.Empty;
            string durabilities = string.Empty;

            foreach (ItemHolder item in m_HotbarCollection)
            {
                if (item.HasItem)
                {
                    itemIds += item.CurrentItem.ItemData.Id + ",";
                    stackSizes += item.CurrentItem.CurrentInStack + ",";
                    durabilities += (!item.CurrentItem.HasProperty("Durability")) ? "-1," : item.CurrentItem.GetPropertyValue("Durability").Float.Current + ",";
                }
                else
                {
                    itemIds += "-1,";
                    stackSizes += "-1,";
                    durabilities += "-1,";
                }
            }

            return itemIds + "|" + stackSizes + "|" + durabilities;
        }

        public string GetEquipDataText()
        {
            string itemIds = string.Empty;
            string stackSizes = string.Empty;
            string durabilities = string.Empty;

            foreach (ItemHolder equipmentHolder in m_EquipmentHolders)
            {
                if (equipmentHolder.HasItem)
                {
                    itemIds += equipmentHolder.CurrentItem.ItemData.Id + ",";
                    stackSizes += equipmentHolder.CurrentItem.CurrentInStack + ",";
                    durabilities += (!equipmentHolder.CurrentItem.HasProperty("Durability")) ? "-1," : equipmentHolder.CurrentItem.GetPropertyValue("Durability").Float.Current + ",";
                }
                else
                {
                    itemIds += "-1,";
                    stackSizes += "-1,";
                    durabilities += "-1,";
                }
            }

            return itemIds + "|" + stackSizes + "|" + durabilities;
        }

        public void SetItemsFromText(string itemdatatext, string collection)
        {
            if (!isLocalPlayer) return;

            string[] dataParts = itemdatatext.Split('|');
            if (dataParts.Length < 3) return;

            // Очищаем коллекцию локально
            RemoveItemsFromCollectionLocal(collection);

            string[] itemIds = dataParts[0].Split(',');
            string[] stackSizes = dataParts[1].Split(',');
            string[] durabilities = dataParts[2].Split(',');

            List<ItemHolder> targetCollection = collection == "Inventory" ? m_InventoryCollection : m_HotbarCollection;

            for (int i = 0; i < itemIds.Length - 1; i++)
            {
                int.TryParse(itemIds[i], out int itemId);
                int.TryParse(stackSizes[i], out int stackSize);
                float.TryParse(durabilities[i], out float durability);

                if (itemId == -1) continue;

                ItemData itemData;
                if (m_ItemDatabase.FindItemById(itemId, out itemData))
                {
                    targetCollection[i].TryAddItem(itemData, stackSize, out int added);

                    if (added > 0 && durability != -1f && targetCollection[i].CurrentItem.HasProperty("Durability"))
                    {
                        ItemProperty.Float durabilityProp = targetCollection[i].CurrentItem.GetPropertyValue("Durability").Float;
                        durabilityProp.Current = durability;
                        targetCollection[i].CurrentItem.GetPropertyValue("Durability").SetValue(ItemProperty.Type.Float, durabilityProp);
                    }
                }
            }
        }

        private void RemoveItemsFromCollectionLocal(string collection)
        {
            List<ItemHolder> targetCollection = collection == "Inventory" ? m_InventoryCollection :
                                              collection == "Hotbar" ? m_HotbarCollection :
                                              m_EquipmentHolders;

            foreach (ItemHolder holder in targetCollection)
            {
                if (holder.HasItem)
                {
                    holder.SetItem(null);
                }
            }
        }

        public void SetEquipFromText(string itemdatatext)
        {
            if (!isLocalPlayer) return;

            string[] dataParts = itemdatatext.Split('|');
            if (dataParts.Length < 3) return;

            // Очищаем экипировку локально
            RemoveItemsFromCollectionLocal("Equipment");

            string[] itemIds = dataParts[0].Split(',');
            string[] stackSizes = dataParts[1].Split(',');
            string[] durabilities = dataParts[2].Split(',');

            int totalDefense = 0;

            for (int i = 0; i < itemIds.Length - 1; i++)
            {
                int.TryParse(itemIds[i], out int itemId);
                int.TryParse(stackSizes[i], out int stackSize);
                float.TryParse(durabilities[i], out float durability);

                if (itemId == -1) continue;

                ItemData itemData;
                if (m_ItemDatabase.FindItemById(itemId, out itemData))
                {
                    m_EquipmentHolders[i].TryAddItem(itemData, stackSize, out int added);

                    if (added > 0)
                    {
                        if (durability != -1f && m_EquipmentHolders[i].CurrentItem.HasProperty("Durability"))
                        {
                            ItemProperty.Float durabilityProp = m_EquipmentHolders[i].CurrentItem.GetPropertyValue("Durability").Float;
                            durabilityProp.Current = durability;
                            m_EquipmentHolders[i].CurrentItem.GetPropertyValue("Durability").SetValue(ItemProperty.Type.Float, durabilityProp);
                        }

                        // Вызываем событие для обратной совместимости
                        EquipmentChanged.Send(m_EquipmentHolders[i]);

                        if (m_EquipmentHolders[i].CurrentItem.HasProperty("Defense"))
                        {
                            totalDefense += m_EquipmentHolders[i].CurrentItem.GetPropertyValue("Defense").Int.Current;
                        }
                    }
                }
            }

            if (playerStats != null)
            {
                playerStats.SetDefense(totalDefense);
            }
        }

        // Вспомогательные методы для вызова из других скриптов
        public void Client_AddItem(int itemID, int amount, string collection)
        {
            if (isLocalPlayer)
            {
                CmdAddItem(itemID, amount, collection);
            }
        }

        public void Client_RemoveItem(string itemName, int amount = 1)
        {
            if (isLocalPlayer)
            {
                CmdRemoveItem(itemName, amount);
            }
        }

        // Перегрузка для удаления одного предмета
        public void Client_RemoveItem(string itemName)
        {
            if (isLocalPlayer)
            {
                CmdRemoveItem(itemName, 1);
            }
        }

        public void Client_DropItem(int slotIndex, string collectionName)
        {
            if (isLocalPlayer)
            {
                CmdDropItem(slotIndex, collectionName);
            }
        }

        // Метод для подбора предметов (вызывается из ItemPickup)
        public void Client_PickupItem(GameObject pickupObject)
        {
            if (isLocalPlayer)
            {
                CmdPickupItem(pickupObject);
            }
        }

        // Для Singleplayer режима или обратной совместимости
        public static InventoryController Instance
        {
            get
            {
                // Если в сцене нет NetworkManager, возвращаем первый найденный InventoryController
                if (NetworkManager.singleton == null)
                {
                    return FindObjectOfType<InventoryController>();
                }
                return null;
            }
        }
    }
}