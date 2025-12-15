using UnityEngine;
using UnityEngine.UI;

public class SlidePanel : MonoBehaviour
{
    public enum SlideDirection { Up, Down, Left, Right }
    
    [Header("Ссылки")]
    public RectTransform panel;
    public Button controlButton;
    
    [Header("Настройки")]
    public SlideDirection direction = SlideDirection.Up;
    public float slideDistance = 300f;
    public float animationDuration = 0.3f;
    
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private bool isVisible = false;
    private float animationTimer = 0f;

    void Start()
    {
        // Вычисляем позиции
        CalculatePositions();
        
        // Устанавливаем начальную позицию
        panel.anchoredPosition = hiddenPosition;
        
        // Назначаем обработчик кнопки
        controlButton.onClick.AddListener(TogglePanel);
    }

    void Update()
    {
        if (animationTimer > 0)
        {
            animationTimer -= Time.deltaTime;
            float progress = 1f - (animationTimer / animationDuration);
            
            panel.anchoredPosition = Vector2.Lerp(
                isVisible ? hiddenPosition : visiblePosition,
                isVisible ? visiblePosition : hiddenPosition,
                progress
            );
        }
    }

    void CalculatePositions()
    {
        hiddenPosition = panel.anchoredPosition;
        
        switch (direction)
        {
            case SlideDirection.Up:
                visiblePosition = hiddenPosition + new Vector2(0, slideDistance);
                break;
            case SlideDirection.Down:
                visiblePosition = hiddenPosition + new Vector2(0, -slideDistance);
                break;
            case SlideDirection.Left:
                visiblePosition = hiddenPosition + new Vector2(-slideDistance, 0);
                break;
            case SlideDirection.Right:
                visiblePosition = hiddenPosition + new Vector2(slideDistance, 0);
                break;
        }
    }

    public void TogglePanel()
    {
        isVisible = !isVisible;
        animationTimer = animationDuration;
    }

    // Для вызова из других скриптов
    public void ShowPanel() 
    { 
        if (!isVisible) TogglePanel(); 
    }
    
    public void HidePanel() 
    { 
        if (isVisible) TogglePanel(); 
    }
}