using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateSurvival
{
    // Основной класс меню исследований
    public class CustomResearchMenu : InteractableObject, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        [Range(1f, 50f)]
        protected int m_ResearchCapacity = 12;

        [SerializeField]
        protected ResearchItem[] m_ResearchItems;

        [Header("UI Settings")]
        [SerializeField]
        private GameObject m_HoverIcon;

        [SerializeField]
        private GameObject m_ResearchPanel;

        [SerializeField]
        private Transform m_ResearchContent;

        [Header("Research Settings")]
        [SerializeField]
        private ResearchItemDisplay m_ResearchItemPrefab;

        [SerializeField]
        private float m_ResearchTimeMultiplier = 1f;

        private bool m_IsPanelOpen = false;
        private Dictionary<string, ResearchItemDisplay> m_ResearchDisplays = new Dictionary<string, ResearchItemDisplay>();
        private PlayerEventHandler m_CurrentPlayer;
        private bool m_WasCursorVisible;
        private CursorLockMode m_PreviousCursorLockState;

        public System.Action<ResearchItem> OnResearchStarted;
        public System.Action<ResearchItem> OnResearchCompleted;
        public System.Action<ResearchItem> OnResearchUnlocked;

        public List<ResearchItem> ResearchItems { get; private set; }
        public bool IsResearchPanelOpen => m_IsPanelOpen;

        public override void OnInteract(PlayerEventHandler player)
        {
            if (base.enabled && !m_IsPanelOpen)
            {
                OpenResearchPanel(player);
            }
            else if (m_IsPanelOpen)
            {
                CloseResearchPanel();
            }
        }

        private void Start()
        {
            if (m_HoverIcon != null)
                m_HoverIcon.SetActive(false);

            if (m_ResearchPanel != null)
                m_ResearchPanel.SetActive(false);

            InitializeResearchItems();
        }

        private void InitializeResearchItems()
        {
            ResearchItems = new List<ResearchItem>();
            
            foreach (var researchData in m_ResearchItems)
            {
                if (researchData != null)
                {
                    var researchInstance = researchData.Clone();
                    ResearchItems.Add(researchInstance);
                }
            }

            SetupResearchDependencies();
        }

        private void SetupResearchDependencies()
        {
            foreach (var research in ResearchItems)
            {
                research.InitializeDependencies(ResearchItems);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_HoverIcon != null && !m_IsPanelOpen)
                m_HoverIcon.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_HoverIcon != null)
                m_HoverIcon.SetActive(false);
        }

        private void OpenResearchPanel(PlayerEventHandler player)
        {
            m_CurrentPlayer = player;
            m_IsPanelOpen = true;
            
            if (m_ResearchPanel != null)
            {
                m_ResearchPanel.SetActive(true);
                PopulateResearchPanel();
            }

            if (m_HoverIcon != null)
                m_HoverIcon.SetActive(false);

            // Сохраняем текущее состояние курсора
            m_WasCursorVisible = Cursor.visible;
            m_PreviousCursorLockState = Cursor.lockState;

            // Блокируем движение игрока через отключение компонентов
            SetPlayerMovement(false);
            
            // Показываем курсор и разблокируем его
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Research panel opened!");
        }

        private void CloseResearchPanel()
        {
            m_IsPanelOpen = false;
            
            if (m_ResearchPanel != null)
                m_ResearchPanel.SetActive(false);

            // Восстанавливаем движение игрока
            SetPlayerMovement(true);

            // Восстанавливаем состояние курсора
            Cursor.lockState = m_PreviousCursorLockState;
            Cursor.visible = m_WasCursorVisible;

            m_CurrentPlayer = null;

            Debug.Log("Research panel closed!");
        }

        private void SetPlayerMovement(bool enabled)
        {
            if (m_CurrentPlayer == null) return;

            // Отключаем/включаем компоненты движения через игровой объект игрока
            var playerGameObject = m_CurrentPlayer.gameObject;

            // Попробуем найти и отключить различные компоненты движения
            var characterController = playerGameObject.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = enabled;
            }

            var rigidbody = playerGameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                if (!enabled)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
                rigidbody.isKinematic = !enabled;
            }

            // Отключаем скрипты движения, если они есть
            var playerMovement = playerGameObject.GetComponent<MonoBehaviour>();
            if (playerMovement != null && playerMovement.enabled != enabled)
            {
                playerMovement.enabled = enabled;
            }
        }

        private void PopulateResearchPanel()
        {
            if (m_ResearchContent == null) return;

            foreach (Transform child in m_ResearchContent)
            {
                Destroy(child.gameObject);
            }
            m_ResearchDisplays.Clear();

            foreach (var research in ResearchItems)
            {
                if (m_ResearchItemPrefab != null)
                {
                    var researchDisplay = Instantiate(m_ResearchItemPrefab, m_ResearchContent);
                    researchDisplay.Initialize(research, this);
                    m_ResearchDisplays.Add(research.ResearchID, researchDisplay);
                }
            }
        }

        public bool StartResearch(ResearchItem research)
        {
            if (research == null) return false;

            if (!research.CanResearch())
                return false;

            if (!research.AreRequirementsMet())
                return false;

            if (research.StartResearch(m_ResearchTimeMultiplier))
            {
                OnResearchStarted?.Invoke(research);
                UpdateResearchDisplay(research);
                return true;
            }

            return false;
        }

        public void CompleteResearch(ResearchItem research)
        {
            if (research == null || !research.IsResearching)
                return;

            research.CompleteResearch();
            OnResearchCompleted?.Invoke(research);
            
            UnlockDependentResearches(research);
            
            UpdateResearchDisplay(research);
        }

        private void UnlockDependentResearches(ResearchItem completedResearch)
        {
            foreach (var research in ResearchItems)
            {
                if (research.IsLocked && research.DependsOnResearch(completedResearch.ResearchID))
                {
                    research.CheckDependencies();
                    if (!research.IsLocked)
                    {
                        OnResearchUnlocked?.Invoke(research);
                        UpdateResearchDisplay(research);
                    }
                }
            }
        }

        private void UpdateResearchDisplay(ResearchItem research)
        {
            if (research != null && m_ResearchDisplays.ContainsKey(research.ResearchID))
            {
                m_ResearchDisplays[research.ResearchID].UpdateDisplay();
            }
        }

        public ResearchItem GetResearch(string researchID)
        {
            return ResearchItems?.Find(r => r.ResearchID == researchID);
        }

        public bool IsResearchCompleted(string researchID)
        {
            var research = GetResearch(researchID);
            return research != null && research.IsResearched;
        }

        public List<ResearchItem> GetAvailableResearches()
        {
            return ResearchItems?.FindAll(r => r.IsUnlocked && !r.IsResearched && !r.IsResearching) ?? new List<ResearchItem>();
        }

        public List<ResearchItem> GetActiveResearches()
        {
            return ResearchItems?.FindAll(r => r.IsResearching) ?? new List<ResearchItem>();
        }

        private void Update()
        {
            if (m_IsPanelOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseResearchPanel();
            }

            UpdateResearchProgress();
        }

        private void UpdateResearchProgress()
        {
            if (ResearchItems == null) return;

            foreach (var research in ResearchItems)
            {
                if (research.IsResearching)
                {
                    research.UpdateResearchProgress(Time.deltaTime);
                    if (research.IsResearchComplete)
                    {
                        CompleteResearch(research);
                    }
                }
            }
        }

        public ResearchSaveData GetSaveData()
        {
            var saveData = new ResearchSaveData();
            
            if (ResearchItems != null)
            {
                foreach (var research in ResearchItems)
                {
                    var researchData = new ResearchItemSaveData
                    {
                        ResearchID = research.ResearchID,
                        IsResearched = research.IsResearched,
                        IsLocked = research.IsLocked,
                        ResearchProgress = research.ResearchProgress
                    };
                    saveData.Researches.Add(researchData);
                }
            }
            
            return saveData;
        }

        public void LoadSaveData(ResearchSaveData saveData)
        {
            if (saveData == null) return;

            foreach (var researchData in saveData.Researches)
            {
                var research = GetResearch(researchData.ResearchID);
                if (research != null)
                {
                    research.LoadSaveData(researchData);
                    UpdateResearchDisplay(research);
                }
            }
        }

        private void OnDestroy()
        {
            if (m_IsPanelOpen)
            {
                CloseResearchPanel();
            }
        }
    }

    // Класс данных исследования - ИСПРАВЛЕННАЯ ВЕРСИЯ
    [System.Serializable]
    public class ResearchItem
    {
        [Header("Basic Settings")]
        public string ResearchID;
        public string ResearchName;
        [TextArea]
        public string ResearchDescription;
        public Sprite ResearchIcon;

        [Header("Research Settings")]
        public float ResearchTime = 60f;
        public List<string> RequiredResearchIDs = new List<string>();
        public List<ItemRequirement> RequiredItems = new List<ItemRequirement>();

        [Header("Research State")]
        public bool IsResearched = false;
        public bool IsResearching = false;
        public bool IsLocked = true;
        public float ResearchProgress = 0f;

        // ЗАМЕНА: используем SerializeReference для предотвращения циклических ссылок
        [SerializeReference]
        private List<ResearchItem> m_RequiredResearches = new List<ResearchItem>();
        
        // УДАЛЕНО: убираем m_DependentResearches чтобы разорвать цикл
        // private List<ResearchItem> m_DependentResearches = new List<ResearchItem>();

        public bool IsUnlocked => !IsLocked;
        public bool IsResearchComplete => ResearchProgress >= ResearchTime;
        public float ResearchPercentage => ResearchTime > 0 ? ResearchProgress / ResearchTime : 0f;

        public ResearchItem Clone()
        {
            return new ResearchItem
            {
                ResearchID = this.ResearchID,
                ResearchName = this.ResearchName,
                ResearchDescription = this.ResearchDescription,
                ResearchIcon = this.ResearchIcon,
                ResearchTime = this.ResearchTime,
                RequiredResearchIDs = new List<string>(this.RequiredResearchIDs),
                RequiredItems = new List<ItemRequirement>(this.RequiredItems),
                IsResearched = this.IsResearched,
                IsResearching = this.IsResearching,
                IsLocked = this.IsLocked,
                ResearchProgress = this.ResearchProgress
            };
        }

        public void InitializeDependencies(List<ResearchItem> allResearches)
        {
            m_RequiredResearches.Clear();
            // m_DependentResearches.Clear() - УДАЛЕНО

            foreach (var requiredID in RequiredResearchIDs)
            {
                var requiredResearch = allResearches.Find(r => r.ResearchID == requiredID);
                if (requiredResearch != null)
                {
                    m_RequiredResearches.Add(requiredResearch);
                    // УДАЛЕНО: убираем обратную ссылку чтобы разорвать цикл
                    // requiredResearch.m_DependentResearches.Add(this);
                }
            }

            CheckDependencies();
        }

        public void CheckDependencies()
        {
            // Если нет требований по исследованиям, разблокируем
            if (m_RequiredResearches.Count == 0)
            {
                IsLocked = false;
                return;
            }

            // Проверяем, все ли требуемые исследования завершены
            bool allRequiredCompleted = true;
            foreach (var required in m_RequiredResearches)
            {
                if (!required.IsResearched)
                {
                    allRequiredCompleted = false;
                    break;
                }
            }

            IsLocked = !allRequiredCompleted;
        }

        public bool AreRequirementsMet()
        {
            if (IsLocked) return false;

            // Упрощенная проверка предметных требований
            // В реальной реализации здесь должна быть проверка инвентаря игрока
            return true;
        }

        public bool CanResearch()
        {
            return !IsResearched && !IsResearching && IsUnlocked && AreRequirementsMet();
        }

        public bool StartResearch(float timeMultiplier = 1f)
        {
            if (!CanResearch()) return false;

            IsResearching = true;
            ResearchProgress = 0f;
            return true;
        }

        public void UpdateResearchProgress(float deltaTime)
        {
            if (IsResearching && !IsResearched)
            {
                ResearchProgress += deltaTime;
                if (ResearchProgress >= ResearchTime)
                {
                    ResearchProgress = ResearchTime;
                }
            }
        }

        public void CompleteResearch()
        {
            IsResearching = false;
            IsResearched = true;
            ResearchProgress = ResearchTime;

            // Применяем эффекты исследования
            ApplyResearchEffects();
        }

        private void ApplyResearchEffects()
        {
            // Здесь применяются эффекты исследования (разблокировка крафта, улучшения и т.д.)
            Debug.Log($"Research completed: {ResearchName}");
        }

        public bool DependsOnResearch(string researchID)
        {
            return m_RequiredResearches.Exists(r => r.ResearchID == researchID);
        }

        public void LoadSaveData(ResearchItemSaveData saveData)
        {
            IsResearched = saveData.IsResearched;
            IsLocked = saveData.IsLocked;
            ResearchProgress = saveData.ResearchProgress;
            
            if (IsResearched)
            {
                ResearchProgress = ResearchTime;
            }
        }
    }

    // Класс для отображения исследования в UI
    public class ResearchItemDisplay : MonoBehaviour
    {
        [Header("UI References")]
        public Image ResearchIcon;
        public Text ResearchNameText;
        public Text ResearchDescriptionText;
        public Button ResearchButton;
        public Slider ProgressSlider;
        public Text ProgressText;
        public GameObject LockedPanel;
        public GameObject CompletedPanel;
        public GameObject ResearchingPanel;

        private ResearchItem m_Research;
        private CustomResearchMenu m_ResearchMenu;

        public void Initialize(ResearchItem research, CustomResearchMenu researchMenu)
        {
            m_Research = research;
            m_ResearchMenu = researchMenu;

            UpdateDisplay();

            if (ResearchButton != null)
            {
                ResearchButton.onClick.RemoveAllListeners();
                ResearchButton.onClick.AddListener(OnResearchButtonClick);
            }
        }

        public void UpdateDisplay()
        {
            if (m_Research == null) return;

            // Основная информация
            if (ResearchIcon != null) ResearchIcon.sprite = m_Research.ResearchIcon;
            if (ResearchNameText != null) ResearchNameText.text = m_Research.ResearchName;
            if (ResearchDescriptionText != null) ResearchDescriptionText.text = m_Research.ResearchDescription;

            // Прогресс
            if (ProgressSlider != null)
            {
                ProgressSlider.gameObject.SetActive(m_Research.IsResearching);
                ProgressSlider.value = m_Research.ResearchPercentage;
            }

            if (ProgressText != null)
            {
                ProgressText.gameObject.SetActive(m_Research.IsResearching);
                ProgressText.text = $"{m_Research.ResearchPercentage:P1}";
            }

            // Статус панели
            if (LockedPanel != null) LockedPanel.SetActive(m_Research.IsLocked);
            if (CompletedPanel != null) CompletedPanel.SetActive(m_Research.IsResearched);
            if (ResearchingPanel != null) ResearchingPanel.SetActive(m_Research.IsResearching);

            // Кнопка исследования
            if (ResearchButton != null)
            {
                ResearchButton.interactable = !m_Research.IsResearched && !m_Research.IsResearching && !m_Research.IsLocked;
            }
        }

        private void OnResearchButtonClick()
        {
            if (m_Research != null && m_ResearchMenu != null)
            {
                m_ResearchMenu.StartResearch(m_Research);
            }
        }
    }

    // Класс требования предмета
    [System.Serializable]
    public class ItemRequirement
    {
        public string ItemName;
        public int Quantity = 1;
        public bool ConsumeOnResearch = true;
    }

    // Классы для сохранения данных
    [System.Serializable]
    public class ResearchSaveData
    {
        public List<ResearchItemSaveData> Researches = new List<ResearchItemSaveData>();
    }

    [System.Serializable]
    public class ResearchItemSaveData
    {
        public string ResearchID;
        public bool IsResearched;
        public bool IsLocked;
        public float ResearchProgress;
    }
}