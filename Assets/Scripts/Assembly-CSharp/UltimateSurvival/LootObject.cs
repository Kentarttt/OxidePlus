using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
    public class LootObject : InteractableObject, IInventoryTrigger
    {
        [SerializeField]
        [Range(1f, 50f)]
        protected int m_Capacity = 8;

        [SerializeField]
        protected ItemToGenerate[] m_InitialItems;

        [Header("Random Loot Settings")]
        [SerializeField]
        private LootTableData[] m_LootTables;

        [SerializeField]
        [Range(0, 100)]
        private int m_EmptySlotChance = 30; // Шанс пустого слота в процентах

        [SerializeField]
        private bool m_UseRandomLoot = true;

        [Header("Box Opening")]
        [SerializeField]
        private Transform m_Cover;

        [SerializeField]
        private float m_OpenSpeed = 6f;

        [SerializeField]
        private float m_ClosedRotation;

        [SerializeField]
        private float m_OpenRotation = 60f;

        private float m_CurrentRotation;

        private int id = -1;

        [SerializeField]
        private bool m_SaveItems;

        // Константа для количества слотов по горизонтали
        public const int HORIZONTAL_SLOTS = 4;

        public List<ItemHolder> ItemHolders { get; private set; }

        // Свойство для получения количества строк
        public int Rows 
        { 
            get 
            { 
                return Mathf.CeilToInt((float)m_Capacity / HORIZONTAL_SLOTS);
            } 
        }

        // Свойство для получения общего количества слотов
        public int SlotCount 
        { 
            get 
            { 
                return m_Capacity; 
            } 
        }

        public override void OnInteract(PlayerEventHandler player)
        {
            if (base.enabled && MonoSingleton<InventoryController>.Instance.OpenLootContainer.Try(this))
            {
                On_InventoryOpened();
                MonoSingleton<InventoryController>.Instance.State.AddChangeListener(OnChanged_InventoryController_State);
            }
        }

        private void Start()
        {
            ItemHolders = new List<ItemHolder>();
            
            if (m_UseRandomLoot)
            {
                GenerateRandomLoot();
            }
            else
            {
                GenerateInitialLoot();
            }
        }

        private void GenerateInitialLoot()
        {
            for (int i = 0; i < m_Capacity; i++)
            {
                ItemHolders.Add(new ItemHolder());
                SavableItem runtimeItem;
                if (i < m_InitialItems.Length && m_InitialItems[i].TryGenerate(out runtimeItem))
                {
                    ItemHolders[i].SetItem(runtimeItem);
                }
            }
        }

        private void GenerateRandomLoot()
        {
            // Сначала заполняем все слоты пустыми холдерами
            for (int i = 0; i < m_Capacity; i++)
            {
                ItemHolders.Add(new ItemHolder());
            }

            // Генерируем случайные предметы
            foreach (var lootTable in m_LootTables)
            {
                if (lootTable == null || lootTable.lootItems.Length == 0)
                    continue;

                // Проверяем шанс появления предмета из этой таблицы
                if (Random.Range(0f, 100f) <= lootTable.spawnChance)
                {
                    GenerateLootFromTable(lootTable);
                }
            }

            // Дополнительно: случайным образом заполняем некоторые слоты
            FillRandomSlots();
        }

        private void GenerateLootFromTable(LootTableData lootTable)
        {
            var availableSlots = GetEmptySlots();
            if (availableSlots.Count == 0)
                return;

            // Выбираем случайные предметы из таблицы лута
            foreach (var lootItem in lootTable.lootItems)
            {
                // Проверяем шанс появления конкретного предмета
                if (Random.Range(0f, 100f) > lootItem.spawnChance)
                    continue;

                // Если нет свободных слотов, выходим
                if (availableSlots.Count == 0)
                    break;

                // Выбираем случайный слот
                int randomSlotIndex = Random.Range(0, availableSlots.Count);
                int slotIndex = availableSlots[randomSlotIndex];
                availableSlots.RemoveAt(randomSlotIndex);

                // Создаем предмет
                ItemData itemData;
                if (MonoSingleton<InventoryController>.Instance.Database.FindItemById(lootItem.itemId, out itemData))
                {
                    // Генерируем случайное количество в пределах min-max
                    int amount = Random.Range(lootItem.minAmount, lootItem.maxAmount + 1);
                    var savableItem = new SavableItem(itemData, amount);
                    
                    ItemHolders[slotIndex].SetItem(savableItem);
                }
            }
        }

        private void FillRandomSlots()
        {
            for (int i = 0; i < ItemHolders.Count; i++)
            {
                // Пропускаем уже заполненные слоты
                if (!ItemHolders[i].HasItem)
                {
                    // Шанс оставить слот пустым
                    if (Random.Range(0, 100) < m_EmptySlotChance)
                        continue;

                    // Пробуем сгенерировать случайный предмет из всех таблиц
                    TryGenerateRandomItemInSlot(i);
                }
            }
        }

        private void TryGenerateRandomItemInSlot(int slotIndex)
        {
            // Собираем все возможные предметы из всех таблиц
            List<WeightedLootItem> possibleItems = new List<WeightedLootItem>();
            
            foreach (var lootTable in m_LootTables)
            {
                if (lootTable == null || Random.Range(0f, 100f) > lootTable.spawnChance)
                    continue;

                foreach (var lootItem in lootTable.lootItems)
                {
                    if (Random.Range(0f, 100f) <= lootItem.spawnChance)
                    {
                        possibleItems.Add(new WeightedLootItem 
                        { 
                            lootItem = lootItem, 
                            weight = lootItem.spawnChance 
                        });
                    }
                }
            }

            // Выбираем случайный предмет на основе весов
            if (possibleItems.Count > 0)
            {
                var selectedItem = SelectRandomWeightedItem(possibleItems);
                ItemData itemData;
                
                if (MonoSingleton<InventoryController>.Instance.Database.FindItemById(selectedItem.itemId, out itemData))
                {
                    int amount = Random.Range(selectedItem.minAmount, selectedItem.maxAmount + 1);
                    var savableItem = new SavableItem(itemData, amount);
                    ItemHolders[slotIndex].SetItem(savableItem);
                }
            }
        }

        private LootItemData SelectRandomWeightedItem(List<WeightedLootItem> weightedItems)
        {
            float totalWeight = 0f;
            foreach (var weightedItem in weightedItems)
            {
                totalWeight += weightedItem.weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var weightedItem in weightedItems)
            {
                currentWeight += weightedItem.weight;
                if (randomValue <= currentWeight)
                {
                    return weightedItem.lootItem;
                }
            }

            return weightedItems[0].lootItem;
        }

        private List<int> GetEmptySlots()
        {
            List<int> emptySlots = new List<int>();
            for (int i = 0; i < ItemHolders.Count; i++)
            {
                if (!ItemHolders[i].HasItem)
                {
                    emptySlots.Add(i);
                }
            }
            return emptySlots;
        }

        private void OnChanged_InventoryController_State()
        {
            if (MonoSingleton<InventoryController>.Instance.IsClosed)
            {
                On_InventoryClosed();
            }
        }

        private void On_InventoryOpened()
        {
            if (m_Cover != null)
            {
                StopAllCoroutines();
                StartCoroutine(C_OpenCover(true));
            }
        }

        private void On_InventoryClosed()
        {
            if (m_Cover != null)
            {
                StopAllCoroutines();
                StartCoroutine(C_OpenCover(false));
            }
        }

        private IEnumerator C_OpenCover(bool open)
        {
            float targetRotation = ((!open) ? m_ClosedRotation : m_OpenRotation);
            while (Mathf.Abs(targetRotation - m_CurrentRotation) > 0.1f)
            {
                m_CurrentRotation = Mathf.Lerp(m_CurrentRotation, targetRotation, Time.deltaTime * m_OpenSpeed);
                m_Cover.transform.localRotation = Quaternion.Euler(m_CurrentRotation, 0f, 0f);
                yield return null;
            }
        }

        public void SaveItems()
        {
            if (m_SaveItems)
            {
                GameController.SceneSave.SaveObjectItems(id);
            }
        }

        public void SetLootItems(int[] id, int[] amount, int[] durability)
        {
            StartCoroutine(C_SetLootItems(id, amount, durability));
        }

        private IEnumerator C_SetLootItems(int[] id, int[] amount, int[] durability)
        {
            while (ItemHolders == null)
            {
                yield return null;
            }
            ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
            for (int i = 0; i < ItemHolders.Count; i++)
            {
                ItemData itemData;
                if (database.FindItemById(id[i], out itemData))
                {
                    ItemHolders[i].SetItem(new SavableItem(itemData, amount[i]));
                }
            }
        }

        public void SetID(int _id)
        {
            id = _id;
        }

        // Метод для получения данных о сетке
        public void GetGridDimensions(out int columns, out int rows)
        {
            columns = HORIZONTAL_SLOTS;
            rows = Rows;
        }
    }

    [System.Serializable]
    public class LootTableData
    {
        public string tableName;
        [Range(0, 100)]
        public float spawnChance = 100f;
        public LootItemData[] lootItems;
    }

    [System.Serializable]
    public class LootItemData
    {
        public string itemName;
        public int itemId;
        [Range(0, 100)]
        public float spawnChance = 100f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    public class WeightedLootItem
    {
        public LootItemData lootItem;
        public float weight;
    }
}