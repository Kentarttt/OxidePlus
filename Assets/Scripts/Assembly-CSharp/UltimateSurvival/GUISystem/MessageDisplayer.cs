using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateSurvival.GUISystem
{
	public class MessageDisplayer : MonoSingleton<MessageDisplayer>
	{
		[SerializeField]
		private GameObject m_MessageTemplate;

		[SerializeField]
		private Color m_BaseMessageColor = Color.yellow;

		[SerializeField]
		private float m_FadeDelay = 3f;

		[SerializeField]
		private float m_FadeSpeed = 0.3f;

		public void PushMessage(string message, Color color = default(Color), int lineHeight = 16)
		{
			if (color == default(Color))
			{
				color = m_BaseMessageColor;
			}
			GameObject gameObject = Object.Instantiate(m_MessageTemplate, m_MessageTemplate.transform.parent, false);
			gameObject.SetActive(true);
			gameObject.transform.SetAsLastSibling();
			Text component = gameObject.GetComponent<Text>();
			CanvasGroup component2 = component.GetComponent<CanvasGroup>();
			if ((bool)component && (bool)component2)
			{
				component.text = message;
				component.color = new Color(color.r, color.g, color.b, 1f);
				component.GetComponent<LayoutElement>().preferredHeight = lineHeight;
				component2.alpha = color.a;
				StartCoroutine(FadeMessage(component2));
			}
		}

		private void Start()
		{
			m_MessageTemplate.SetActive(false);
		}

		private IEnumerator FadeMessage(CanvasGroup group)
		{
			if ((bool)group)
			{
				yield return new WaitForSeconds(m_FadeDelay);
				while (group.alpha > Mathf.Epsilon)
				{
					group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.deltaTime * m_FadeSpeed);
					yield return null;
				}
				Object.Destroy(group.gameObject);
			}
		}
	}
}
