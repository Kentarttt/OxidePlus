using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class AudioUtils : MonoBehaviour
	{
		public Value<Gunshot> LastGunshot = new Value<Gunshot>(null);

		private Dictionary<AudioSource, Coroutine> m_LevelSetters = new Dictionary<AudioSource, Coroutine>();

		[SerializeField]
		private AudioSource m_2DAudioSource;

		public void Play2D(AudioClip clip, float volume)
		{
			if ((bool)m_2DAudioSource)
			{
				m_2DAudioSource.PlayOneShot(clip, volume);
			}
		}

		public AudioSource CreateAudioSource(string name, Transform parent, Vector3 localPosition, bool is2D, float startVolume, float minDistance)
		{
			GameObject gameObject = new GameObject(name, typeof(AudioSource));
			if (!parent)
			{
				parent = base.transform;
			}
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = localPosition;
			AudioSource component = gameObject.GetComponent<AudioSource>();
			component.volume = startVolume;
			component.spatialBlend = ((!is2D) ? 1f : 0f);
			component.minDistance = minDistance;
			return component;
		}

		public void LerpVolumeOverTime(AudioSource audioSource, float targetVolume, float speed)
		{
			if (m_LevelSetters.ContainsKey(audioSource))
			{
				if (m_LevelSetters[audioSource] != null)
				{
					StopCoroutine(m_LevelSetters[audioSource]);
				}
				m_LevelSetters[audioSource] = StartCoroutine(C_LerpVolumeOverTime(audioSource, targetVolume, speed));
			}
			else
			{
				m_LevelSetters.Add(audioSource, StartCoroutine(C_LerpVolumeOverTime(audioSource, targetVolume, speed)));
			}
		}

		private IEnumerator C_LerpVolumeOverTime(AudioSource audioSource, float volume, float speed)
		{
			while (audioSource != null && Mathf.Abs(audioSource.volume - volume) > 0.01f)
			{
				audioSource.volume = Mathf.MoveTowards(audioSource.volume, volume, Time.deltaTime * speed);
				yield return null;
			}
			m_LevelSetters.Remove(audioSource);
		}
	}
}
