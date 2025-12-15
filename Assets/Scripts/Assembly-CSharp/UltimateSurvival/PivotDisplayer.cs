using UnityEngine;

namespace UltimateSurvival
{
	public class PivotDisplayer : MonoBehaviour
	{
		[SerializeField]
		private Color m_Color = Color.red;

		[SerializeField]
		private float m_Radius = 0.06f;

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = m_Color;
			Gizmos.DrawSphere(base.transform.position, m_Radius);
		}
	}
}
