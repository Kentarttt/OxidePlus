using System.Collections.Generic;
using UltimateSurvival.GUISystem;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival
{
    public class MineableObject : MonoBehaviour
    {
        public Message Destroyed = new Message();

        [SerializeField]
        private FPTool.ToolPurpose m_RequiredToolPurpose;

        [Range(0.01f, 1f)]
        [SerializeField]
        private float m_Resistance = 0.5f;

        [Header("On Destroy")]
        [SerializeField]
        private GameObject m_DestroyedObject;

        [SerializeField]
        private Vector3 m_DestroyedObjectOffset;

        [Header("Loot")]
        [SerializeField]
        private LootItem[] m_Loot;

        [SerializeField]
        private bool m_ShowGatherMessage = true;

        [SerializeField]
        private Color m_MessageColor = Color.white;

        [SerializeField]
        private Color m_BackgroundColor = new Color(0f, 0.4f, 0f, 0.9f);

        // Статический список для управления всеми сообщениями
        private static List<GatherMessage> m_ActiveMessages = new List<GatherMessage>();
        
        // Настройки сообщений
        private const float MESSAGE_SPACING = 35f;
        private const int MAX_MESSAGES = 5;

        // Статический Canvas для всех сообщений
        private static Canvas m_MessageCanvas;

        private float m_CurrentHealth = 100f;

        public float Health
        {
            get
            {
                return m_CurrentHealth;
            }
        }

        private void Start()
        {
            // Создаем Canvas при первом использовании
            CreateCanvasIfNeeded();
        }

        private static void CreateCanvasIfNeeded()
        {
            if (m_MessageCanvas == null)
            {
                GameObject canvasGO = new GameObject("GatherMessagesCanvas");
                m_MessageCanvas = canvasGO.AddComponent<Canvas>();
                m_MessageCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                m_MessageCanvas.sortingOrder = 1000;
            }
        }

        public void OnToolHit(FPTool.ToolPurpose[] toolPurposes, float damage, float efficiency)
        {
            bool flag = false;
            foreach (FPTool.ToolPurpose toolPurpose in toolPurposes)
            {
                if (toolPurpose == m_RequiredToolPurpose)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                ReceiveDamage(damage);
                if (m_Loot.Length > 0)
                {
                    GiveLootToPlayer(efficiency);
                }
            }
        }

        private void GiveLootToPlayer(float amountFactor)
        {
            int num = ProbabilityUtils.RandomChoiceFollowingDistribution(GetLootProbabilities());
            LootItem lootItem = m_Loot[num];
            int added;
            lootItem.AddToInventory(out added, amountFactor);
            
            if (m_ShowGatherMessage && added > 0)
            {
                ShowRustStyleGatherMessage(lootItem.ItemName, added);
            }
        }

        private void ShowRustStyleGatherMessage(string itemName, int amount)
        {
            string message = $"{itemName} x{amount}";
            CreateRustStyleGatherUI(message);
        }

        private void CreateRustStyleGatherUI(string message)
        {
            CreateCanvasIfNeeded();

            // Удаляем самое старое сообщение если достигли лимита
            if (m_ActiveMessages.Count >= MAX_MESSAGES)
            {
                var oldestMessage = m_ActiveMessages[0];
                m_ActiveMessages.RemoveAt(0);
                if (oldestMessage != null && oldestMessage.gameObject != null)
                    Destroy(oldestMessage.gameObject);
            }

            // Создаем панельку
            GameObject panel = new GameObject("GatherPanel");
            panel.transform.SetParent(m_MessageCanvas.transform);
            
            // Настраиваем RectTransform панельки
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(140f, 30f);
            
            // ЛЕВЫЙ ВЕРХНИЙ УГОЛ с отступом
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            
            // Добавляем background
            var bgImage = panel.AddComponent<Image>();
            bgImage.color = m_BackgroundColor;

            // Добавляем текст
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(panel.transform);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = message;
            textComponent.color = m_MessageColor;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontSize = 14;
            textComponent.fontStyle = FontStyle.Bold;

            // Добавляем компонент для управления сообщением
            GatherMessage gatherMessage = panel.AddComponent<GatherMessage>();
            gatherMessage.Initialize(panel, m_ActiveMessages.Count);

            // Добавляем в список активных сообщений
            m_ActiveMessages.Add(gatherMessage);

            // Обновляем позиции всех сообщений
            UpdateAllMessagesPositions();

            // Удаляем через время
            Destroy(panel, 3f);
        }

        // Метод для обновления позиций всех сообщений
        private static void UpdateAllMessagesPositions()
        {
            for (int i = 0; i < m_ActiveMessages.Count; i++)
            {
                if (m_ActiveMessages[i] != null)
                {
                    // Позиционируем каждое сообщение в ЛЕВОМ ВЕРХНЕМ УГЛУ
                    RectTransform rectTransform = m_ActiveMessages[i].GetComponent<RectTransform>();
                    float yOffset = -i * MESSAGE_SPACING;
                    rectTransform.anchoredPosition = new Vector2(20f, -50f + yOffset);
                }
            }
        }

        // Статический метод для удаления сообщения из списка
        public static void RemoveMessage(GatherMessage message)
        {
            m_ActiveMessages.Remove(message);
            UpdateAllMessagesPositions();
        }

        private void ReceiveDamage(float damage)
        {
            m_CurrentHealth += (0f - damage) * (1f - m_Resistance);
            if (m_CurrentHealth < Mathf.Epsilon)
            {
                DestroyObject();
            }
        }

        private void DestroyObject()
        {
            if ((bool)m_DestroyedObject)
            {
                Object.Instantiate(m_DestroyedObject, base.transform.position + base.transform.TransformVector(m_DestroyedObjectOffset), Quaternion.identity);
            }
            Object.Destroy(base.gameObject);
            if (Destroyed != null)
            {
                Destroyed.Send();
            }
        }

        private List<float> GetLootProbabilities()
        {
            List<float> list = new List<float>();
            for (int i = 0; i < m_Loot.Length; i++)
            {
                list.Add(m_Loot[i].SpawnChance);
            }
            return list;
        }
    }

    // Класс для управления отдельным сообщением
    public class GatherMessage : MonoBehaviour
    {
        private int m_Index;

        public void Initialize(GameObject messageUI, int index)
        {
            m_Index = index;
        }

        private void OnDestroy()
        {
            // При удалении сообщения обновляем позиции остальных
            MineableObject.RemoveMessage(this);
        }
    }
}