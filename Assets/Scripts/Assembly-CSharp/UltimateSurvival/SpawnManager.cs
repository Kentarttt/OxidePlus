using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance;
        
        [System.Serializable]
        public class SpawnPoint
        {
            public string name;
            public Vector3 position;
            public bool isDefault = false;
            public string description = "";
        }
        
        [Header("Spawn Points")]
        public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        
        [Header("Settings")]
        public bool debugMode = true;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultSpawnPoints();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeDefaultSpawnPoints()
        {
            if (spawnPoints.Count == 0)
            {
                spawnPoints.Add(new SpawnPoint { 
                    name = "MainSpawn", 
                    position = new Vector3(0, 5, 0), 
                    isDefault = true,
                    description = "Основная точка спавна" 
                });
                
                spawnPoints.Add(new SpawnPoint { 
                    name = "ForestSpawn", 
                    position = new Vector3(50, 10, 30), 
                    isDefault = false,
                    description = "Лесная зона" 
                });
                
                spawnPoints.Add(new SpawnPoint { 
                    name = "BeachSpawn", 
                    position = new Vector3(-25, 3, -40), 
                    isDefault = false,
                    description = "Пляж" 
                });
            }
        }
        
        public Vector3 GetSpawnPosition(string spawnName = "")
        {
            SpawnPoint spawnPoint = null;
            
            if (!string.IsNullOrEmpty(spawnName))
            {
                spawnPoint = spawnPoints.Find(p => p.name == spawnName);
                if (spawnPoint != null)
                {
                    if (debugMode) Debug.Log($"Spawn point found: {spawnPoint.name}");
                }
                else
                {
                    if (debugMode) Debug.LogWarning($"Spawn point '{spawnName}' not found, using default");
                }
            }
            
            if (spawnPoint == null)
            {
                spawnPoint = spawnPoints.Find(p => p.isDefault);
                if (spawnPoint != null && debugMode) 
                    Debug.Log($"Using default spawn point: {spawnPoint.name}");
            }
            
            if (spawnPoint == null && spawnPoints.Count > 0)
            {
                spawnPoint = spawnPoints[0];
                if (debugMode) Debug.Log($"Using first spawn point: {spawnPoint.name}");
            }
            
            if (spawnPoint == null)
            {
                if (debugMode) Debug.LogWarning("No spawn points available, using Vector3.zero");
                return Vector3.zero;
            }
            
            return spawnPoint.position;
        }
        
        public Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints.Count == 0)
            {
                if (debugMode) Debug.LogWarning("No spawn points available for random selection");
                return Vector3.zero;
            }
            
            int randomIndex = Random.Range(0, spawnPoints.Count);
            SpawnPoint randomPoint = spawnPoints[randomIndex];
            
            if (debugMode) Debug.Log($"Selected random spawn point: {randomPoint.name}");
            return randomPoint.position;
        }
        
        public void AddSpawnPoint(string name, Vector3 position, bool isDefault = false, string description = "")
        {
            if (isDefault)
            {
                foreach (var point in spawnPoints)
                {
                    point.isDefault = false;
                }
            }
            
            SpawnPoint existingPoint = spawnPoints.Find(p => p.name == name);
            if (existingPoint != null)
            {
                existingPoint.position = position;
                existingPoint.isDefault = isDefault;
                existingPoint.description = description;
                if (debugMode) Debug.Log($"Updated spawn point: {name}");
            }
            else
            {
                spawnPoints.Add(new SpawnPoint { 
                    name = name, 
                    position = position, 
                    isDefault = isDefault,
                    description = description 
                });
                if (debugMode) Debug.Log($"Added new spawn point: {name}");
            }
        }
        
        public bool RemoveSpawnPoint(string name)
        {
            SpawnPoint pointToRemove = spawnPoints.Find(p => p.name == name);
            if (pointToRemove != null)
            {
                spawnPoints.Remove(pointToRemove);
                if (debugMode) Debug.Log($"Removed spawn point: {name}");
                return true;
            }
            
            if (debugMode) Debug.LogWarning($"Spawn point '{name}' not found for removal");
            return false;
        }
        
        public List<string> GetAllSpawnPointNames()
        {
            List<string> names = new List<string>();
            foreach (var point in spawnPoints)
            {
                names.Add(point.name);
            }
            return names;
        }
        
        public SpawnPoint GetSpawnPointInfo(string name)
        {
            return spawnPoints.Find(p => p.name == name);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;
            
            foreach (var point in spawnPoints)
            {
                Gizmos.color = point.isDefault ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(point.position, 1f);
            }
        }
    }
}