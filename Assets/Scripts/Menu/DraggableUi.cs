using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 defaultPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private string saveKey;

    private string playerPrefsKey = "MySliderValue";

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        defaultPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y);
        GetPosition();
    }

    private void GetPosition()
    {
        saveKey = gameObject.name + "_Pos";
        float x = PlayerPrefs.GetFloat(saveKey + "_x", rectTransform.anchoredPosition.x);
        float y = PlayerPrefs.GetFloat(saveKey + "_y", rectTransform.anchoredPosition.y);
        rectTransform.anchoredPosition = new Vector2(x, y);
    }

    public void OnBeginDrag(PointerEventData eventData) {  }

    public void OnDrag(PointerEventData eventData) { rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; }

    public void OnEndDrag(PointerEventData eventData) { Vector2 pos = rectTransform.anchoredPosition; }

    public void SavePosition()
    {
        PlayerPrefs.SetFloat(saveKey + "_x", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(saveKey + "_y", rectTransform.anchoredPosition.y);
        PlayerPrefs.SetFloat(playerPrefsKey + "Size", rectTransform.localScale.x);
        PlayerPrefs.Save();
    }

    public void CancelPosition()
    {
        float x = PlayerPrefs.GetFloat(saveKey + "_x", rectTransform.anchoredPosition.x);
        float y = PlayerPrefs.GetFloat(saveKey + "_y", rectTransform.anchoredPosition.y);
        rectTransform.anchoredPosition = new Vector2(x, y);
        ControlSliderScript.instance.SaveValue(PlayerPrefs.GetFloat(playerPrefsKey + "Size", 1.0f));
    }

    public void DefaultPosition()
    {
        float x = defaultPosition.x;
        float y = defaultPosition.y;
        rectTransform.anchoredPosition = new Vector2(x, y);
        ButtonScaler.instance.UpdateButtonSizes(1.0f);
    }
}