using UnityEngine;
using UnityEngine.UI;

class loadSpinner : MonoBehaviour
{
    private Image imageComponent;
    [SerializeField] private Sprite[] sprites;          
    [SerializeField] private float switchTime = 0.2f;     

    private int currentIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        imageComponent.sprite = sprites[0];
    }

    private void Update()
    {
        if (sprites.Length < 2) return;

        timer += Time.deltaTime;
        if (timer >= switchTime)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % sprites.Length;
            imageComponent.sprite = sprites[currentIndex];
        }
    }
}