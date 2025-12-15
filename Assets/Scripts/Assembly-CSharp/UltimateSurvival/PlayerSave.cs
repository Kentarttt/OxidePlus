using System.Collections.Generic;
using UltimateSurvival.GUISystem;
using UnityEngine;

namespace UltimateSurvival
{
    public class PlayerSave : PlayerBehaviour
    {
        public List<PlayerSaveData> LoadedData = new List<PlayerSaveData>();
        public Hotbar hotbar;

        [Header("Save Settings")]
        private int SaveInterval = 5;
        private float timeTemp;

        [Header("Spawn Settings")]
        public string defaultSpawnPoint = "MainSpawn";
        public bool useRandomSpawnForNewPlayers = false;
        public bool debugMode = true;

        private void Start()
        {
            // Убедимся что SpawnManager существует
            if (SpawnManager.Instance == null)
            {
                GameObject spawnManagerObj = new GameObject("SpawnManager");
                spawnManagerObj.AddComponent<SpawnManager>();
            }

            LoadPlayerData();
            if (GameController.LocalPlayer.PlaceObject != null)
            {
                GameController.LocalPlayer.PlaceObject.AddListener(SaveHotbarData);
            }
            timeTemp = Time.time;
        }

        private void Update()
        {
            if (Time.time >= timeTemp + (float)SaveInterval)
            {
                timeTemp = Time.time;
                SavePlayer();
            }
        }

        public void SavePlayer()
        {
            PlayerSaveData playersave = new PlayerSaveData();
            playersave.ItemData = MonoSingleton<InventoryController>.Instance.GetItemDataText();
            playersave.EquipData = MonoSingleton<InventoryController>.Instance.GetEquipDataText();
            playersave.HotbarData = MonoSingleton<InventoryController>.Instance.GetHotbarDataText();
            playersave.FPSItemIndex = hotbar.GetSelectedItemIndex();
            
            // Сохранение позиции с улучшенным форматированием
            Vector3 playerPosition = base.transform.position;
            playersave.Position = $"{playerPosition.x:F4},{playerPosition.y:F4},{playerPosition.z:F4}";
            
            playersave.Health = base.Player.Health.Get();
            playersave.Stamina = base.Player.Stamina.Get();
            playersave.Food = base.Player.Hunger.Get();
            playersave.Water = base.Player.Thirst.Get();
            
            timeTemp = Time.time;
            WriteData(playersave);

            // Сохраняем точку спавна отдельно
            PlayerPrefs.SetString("LAST_SPAWN_POINT", defaultSpawnPoint);
            PlayerPrefs.Save();

            if (debugMode) Debug.Log("Player data saved successfully");
        }

        public void SaveHotbarData()
        {
            PlayerPrefs.SetString("HOTBAR", MonoSingleton<InventoryController>.Instance.GetHotbarDataText());
            PlayerPrefs.Save();
        }

        public void LoadPlayerData()
        {
            PlayerSaveData playerSaveData = ReadData();
            
            if (playerSaveData.Health > 0f && !string.IsNullOrEmpty(playerSaveData.Position))
            {
                // Загрузка существующего игрока
                LoadPlayerPosition(playerSaveData.Position);
                
                base.Player.Health.Set(playerSaveData.Health);
                base.Player.Stamina.Set(playerSaveData.Stamina);
                base.Player.Hunger.Set(playerSaveData.Food);
                base.Player.Thirst.Set(playerSaveData.Water);
                
                MonoSingleton<InventoryController>.Instance.SetItemsFromText(playerSaveData.ItemData, "Inventory");
                MonoSingleton<InventoryController>.Instance.SetItemsFromText(playerSaveData.HotbarData, "Hotbar");
                MonoSingleton<InventoryController>.Instance.SetEquipFromText(playerSaveData.EquipData);
                hotbar.TrySelectSlot(playerSaveData.FPSItemIndex);

                // Загружаем точку спавна
                defaultSpawnPoint = PlayerPrefs.GetString("LAST_SPAWN_POINT", "MainSpawn");

                if (debugMode) Debug.Log("Existing player data loaded");
            }
            else
            {
                // НОВЫЙ ИГРОК - спавн в выбранной точке
                SpawnPlayer();
                
                int added;
                MonoSingleton<InventoryController>.Instance.AddItemToCollection(23, 1, "Hotbar", out added);
                hotbar.TrySelectSlot(0);

                if (debugMode) Debug.Log("New player spawned at default location");
            }
        }

        private void LoadPlayerPosition(string positionData)
        {
            if (!string.IsNullOrEmpty(positionData))
            {
                string[] array = positionData.Split(',');
                if (array.Length == 3)
                {
                    Vector3 loadedPosition = Vector3.zero;
                    if (float.TryParse(array[0], out loadedPosition.x) &&
                        float.TryParse(array[1], out loadedPosition.y) &&
                        float.TryParse(array[2], out loadedPosition.z))
                    {
                        TeleportPlayer(loadedPosition);
                        if (debugMode) Debug.Log($"Position loaded: {loadedPosition}");
                    }
                    else
                    {
                        if (debugMode) Debug.LogError("Failed to parse position data, using spawn point");
                        SpawnPlayer();
                    }
                }
                else
                {
                    if (debugMode) Debug.LogError($"Invalid position format. Expected 3 values, got {array.Length}");
                    SpawnPlayer();
                }
            }
            else
            {
                if (debugMode) Debug.LogWarning("No position data found, using spawn point");
                SpawnPlayer();
            }
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPosition;
            
            if (useRandomSpawnForNewPlayers)
            {
                spawnPosition = SpawnManager.Instance.GetRandomSpawnPosition();
                if (debugMode) Debug.Log("Using random spawn point for new player");
            }
            else
            {
                spawnPosition = SpawnManager.Instance.GetSpawnPosition(defaultSpawnPoint);
                if (debugMode) Debug.Log($"Using default spawn point: {defaultSpawnPoint}");
            }
            
            TeleportPlayer(spawnPosition);
            
            // Сохраняем точку спавна
            PlayerPrefs.SetString("LAST_SPAWN_POINT", defaultSpawnPoint);
            PlayerPrefs.Save();
        }

        private void TeleportPlayer(Vector3 position)
        {
            CharacterController characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
                base.transform.position = position;
                characterController.enabled = true;
            }
            else
            {
                base.transform.position = position;
            }
        }

        // === PUBLIC METHODS FOR SPAWN CONTROL ===

        public void ChangeSpawnPoint(string newSpawnPoint)
        {
            defaultSpawnPoint = newSpawnPoint;
            PlayerPrefs.SetString("LAST_SPAWN_POINT", newSpawnPoint);
            PlayerPrefs.Save();
            
            if (debugMode) Debug.Log($"Spawn point changed to: {newSpawnPoint}");
        }

        public void TeleportToSpawnPoint(string spawnPointName = "")
        {
            Vector3 spawnPosition;
            
            if (string.IsNullOrEmpty(spawnPointName))
            {
                spawnPosition = SpawnManager.Instance.GetSpawnPosition(defaultSpawnPoint);
            }
            else
            {
                spawnPosition = SpawnManager.Instance.GetSpawnPosition(spawnPointName);
            }
            
            TeleportPlayer(spawnPosition);
            if (debugMode) Debug.Log($"Teleported to spawn point: {spawnPointName}");
        }

        public void TeleportToRandomSpawnPoint()
        {
            Vector3 spawnPosition = SpawnManager.Instance.GetRandomSpawnPosition();
            TeleportPlayer(spawnPosition);
            if (debugMode) Debug.Log("Teleported to random spawn point");
        }

        public void AddNewSpawnPoint(string name, Vector3 position, bool makeDefault = false, string description = "")
        {
            SpawnManager.Instance.AddSpawnPoint(name, position, makeDefault, description);
            
            if (makeDefault)
            {
                ChangeSpawnPoint(name);
            }
        }

        public List<string> GetAvailableSpawnPoints()
        {
            return SpawnManager.Instance.GetAllSpawnPointNames();
        }

        // === DATA SAVE/LOAD METHODS ===

        public void WriteData(PlayerSaveData playersave)
        {
            PlayerPrefs.SetString("ITEM", playersave.ItemData);
            PlayerPrefs.SetString("EQUIP", playersave.EquipData);
            PlayerPrefs.SetString("HOTBAR", playersave.HotbarData);
            PlayerPrefs.SetInt("FPSINDEX", playersave.FPSItemIndex);
            PlayerPrefs.SetString("POS", playersave.Position);
            PlayerPrefs.SetFloat("FOOD", playersave.Food);
            PlayerPrefs.SetFloat("WATER", playersave.Water);
            PlayerPrefs.SetFloat("STAMINA", playersave.Stamina);
            PlayerPrefs.SetFloat("HEALTH", playersave.Health);
            PlayerPrefs.Save();
        }

        public PlayerSaveData ReadData()
        {
            PlayerSaveData result = new PlayerSaveData();
            result.ItemData = PlayerPrefs.GetString("ITEM");
            result.EquipData = PlayerPrefs.GetString("EQUIP");
            result.HotbarData = PlayerPrefs.GetString("HOTBAR");
            result.FPSItemIndex = PlayerPrefs.GetInt("FPSINDEX");
            result.Position = PlayerPrefs.GetString("POS");
            result.Food = PlayerPrefs.GetFloat("FOOD");
            result.Water = PlayerPrefs.GetFloat("WATER");
            result.Stamina = PlayerPrefs.GetFloat("STAMINA");
            result.Health = PlayerPrefs.GetFloat("HEALTH");
            return result;
        }

        public float TimeToSave()
        {
            return timeTemp + (float)SaveInterval - Time.time;
        }

        // === DEBUG METHODS ===

        private void OnGUI()
        {
            if (debugMode)
            {
                GUI.Box(new Rect(10, 10, 200, 100), "Spawn System Debug");
                GUI.Label(new Rect(20, 30, 180, 20), $"Spawn Point: {defaultSpawnPoint}");
                GUI.Label(new Rect(20, 50, 180, 20), $"Position: {transform.position}");
                GUI.Label(new Rect(20, 70, 180, 20), $"Save in: {TimeToSave():F1}s");
            }
        }
    }
}