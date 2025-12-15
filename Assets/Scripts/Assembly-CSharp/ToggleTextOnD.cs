using UnityEngine;
using UnityEngine.UI;

public class ToggleTextOnD : MonoBehaviour
{
    public Text textComponent;
    private bool isOpen = false;
    
    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<Text>();
        }
        
        UpdateText();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            isOpen = !isOpen; // Переключаем состояние
            UpdateText();
        }
    }
    
    void UpdateText()
    {
        if (textComponent != null)
        {
            textComponent.text = isOpen ? "открыт" : "закрыт";
        }
    }
}