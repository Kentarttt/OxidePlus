using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	public class PooledObject : MonoBehaviour
	{
		public Message<PooledObject> Released = new Message<PooledObject>();

		[SerializeField]
		private bool m_ReleaseOnTimer = true;

		[SerializeField]
		private float m_ReleaseTimer = 20f;

		[SerializeField]
		private ParticleSystem[] m_ToResetParticles;

		private WaitForSeconds m_WaitInterval;

		public virtual void OnUse(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null)
		{
			try
			{
				base.gameObject.SetActive(true);
				base.transform.position = position;
				base.transform.rotation = rotation;
				if ((bool)base.transform.parent)
				{
					base.transform.SetParent(parent);
				}
				for (int i = 0; i < m_ToResetParticles.Length; i++)
				{
					m_ToResetParticles[i].Play(true);
				}
				if (m_ReleaseOnTimer)
				{
					StopAllCoroutines();
					StartCoroutine(ReleaseWithDelay());
				}
			}
			catch
			{
			}
		}

		public virtual void OnRelease()
		{

			base.gameObject.SetActive(false);
			Released.Send(this);
		}

		private void Awake()
		{
			if (m_ReleaseOnTimer)
			{
				m_WaitInterval = new WaitForSeconds(m_ReleaseTimer);
			}
		}

		private IEnumerator ReleaseWithDelay()
		{
			yield return m_WaitInterval;
			OnRelease();
		}
	}
}
