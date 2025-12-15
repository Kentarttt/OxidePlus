using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival
{
	public static class GUIUtils
	{
		public static Text CreateTextUnder(string name, RectTransform parent, TextAnchor anchor, Vector2 offset)
		{
			Text component = new GameObject(name, typeof(Text)).GetComponent<Text>();
			component.transform.SetParent(parent);
			component.transform.localPosition = offset;
			component.transform.localScale = Vector3.one;
			component.rectTransform.pivot = Vector2.one * 0.5f;
			component.rectTransform.sizeDelta = parent.sizeDelta;
			component.alignment = anchor;
			return component;
		}

		public static Image CreateImageUnder(string name, RectTransform parent, Vector2 offset, Vector2 size)
		{
			Image component = new GameObject(name, typeof(Image)).GetComponent<Image>();
			component.transform.SetParent(parent);
			component.transform.localPosition = offset;
			component.transform.localScale = Vector3.one;
			component.rectTransform.pivot = Vector2.one * 0.5f;
			component.rectTransform.sizeDelta = size;
			component.raycastTarget = false;
			return component;
		}
	}
}
