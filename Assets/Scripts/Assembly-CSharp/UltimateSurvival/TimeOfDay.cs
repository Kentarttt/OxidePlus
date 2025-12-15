using UnityEngine;

namespace UltimateSurvival
{
	public class TimeOfDay : MonoSingleton<TimeOfDay>
	{
		public Value<ET.TimeOfDay> State = new Value<ET.TimeOfDay>(ET.TimeOfDay.Day);

		[Header("Setup")]
		[SerializeField]
		private Light m_Sun;

		[SerializeField]
		private Light m_Moon;

		[Header("General")]
		[SerializeField]
		private bool m_StopTime;

		[SerializeField]
		private bool m_ShowGUI;

		[SerializeField]
		[Range(0f, 24f)]
		[Tooltip("The current hour (00:00 AM to 12:00 PM to 24:00 PM)")]
		private int m_CurrentHour = 6;

		[SerializeField]
		[Range(0f, 24f)]
		[Tooltip("The current hour (00:00 AM to 12:00 PM to 24:00 PM)")]
		private int m_DefaultHour = 10;

		[SerializeField]
		[Tooltip("How many seconds are in a day.")]
		private int m_DayDuration = 900;

		[SerializeField]
		[Tooltip("On which axis should the moon and sun rotate?")]
		private Vector3 m_RotationAxis = Vector2.right;

		[Header("Fog")]
		[SerializeField]
		private FogMode m_FogMode = FogMode.ExponentialSquared;

		[SerializeField]
		[Tooltip("Fog intensity variation over the whole day & night cycle.")]
		private AnimationCurve m_FogIntensity;

		[SerializeField]
		[Tooltip("Fog color variation over the whole day & night cycle.")]
		private Gradient m_FogColor;

		[SerializeField]
		private Gradient m_AmbientLightColor;

		[Header("Sun")]
		[SerializeField]
		[Tooltip("Sun intensity variation over the whole day & night cycle.")]
		private AnimationCurve m_SunIntensity;

		[SerializeField]
		[Tooltip("Sun color variation over the whole day & night cycle.")]
		private Gradient m_SunColor;

		[Header("Moon")]
		[SerializeField]
		[Tooltip("Moon intensity variation over the whole day & night cycle.")]
		private AnimationCurve m_MoonIntensity;

		[SerializeField]
		[Tooltip("Moon color variation over the whole day & night cycle.")]
		private Gradient m_MoonColor;

		[Header("Skybox")]
		[SerializeField]
		private Material m_Skybox;

		[SerializeField]
		private AnimationCurve m_SkyboxBlend;

		private ET.TimeOfDay m_InternalState;

		private Transform m_SunTransform;

		private Transform m_MoonTransform;

		private float m_NormalizedTime;

		private float m_TimeIncrement;

		private int UpdateInterval = 5;

		private float timeTemp;

		public float NormalizedTime
		{
			get
			{
				return m_NormalizedTime;
			}
			set
			{
				m_NormalizedTime = ((!float.IsNaN(Mathf.Repeat(value, 1f))) ? Mathf.Repeat(value, 1f) : 0f);
				m_CurrentHour = (int)(m_NormalizedTime * 24f);
			}
		}

		public int CurrentHour
		{
			get
			{
				return m_CurrentHour;
			}
		}

		private void Awake()
		{
			if (!m_Sun || !m_Moon)
			{
				Debug.LogError("The moon or sun are not assigned in the inspector! please assign them and restart the game.", this);
				base.enabled = false;
				return;
			}
			m_SunTransform = m_Sun.transform;
			m_MoonTransform = m_Moon.transform;
			AccommodateEditorChanges();
			if (PlayerPrefs.HasKey("DAY_TIME"))
			{
				m_NormalizedTime = PlayerPrefs.GetFloat("DAY_TIME");
			}
			else
			{
				m_NormalizedTime = (float)m_DefaultHour / 24f;
			}
			m_InternalState = ((!NormalizedTime.IsInRangeLimitsExcluded(0.25f, 0.75f)) ? ET.TimeOfDay.Night : ET.TimeOfDay.Day);
			State.Set(m_InternalState);
		}

		private void OnGUI()
		{
			if (m_ShowGUI)
			{
				Rect position = new Rect(8f, 8f, 128f, 20f);
				m_StopTime = GUI.Toggle(position, m_StopTime, "Stop Time?");
				string text = "Time: " + m_CurrentHour + " ";
				text = ((!m_NormalizedTime.IsInRangeLimitsIncluded(0.5f, 1f)) ? (text + "AM") : (text + "PM"));
				position.y = position.yMax + 4f;
				GUI.Label(position, text);
				position.y = position.yMax;
				if (m_StopTime)
				{
					m_NormalizedTime = GUI.HorizontalSlider(position, m_NormalizedTime, 0f, 1f);
				}
				else
				{
					GUI.HorizontalSlider(position, m_NormalizedTime, 0f, 1f);
				}
				position.y = position.yMax;
				position.width = 256f;
				GUI.Label(position, "Day Duration: " + m_DayDuration + " seconds");
				position.y = position.yMax;
				position.width = 128f;
				m_DayDuration = (int)GUI.HorizontalSlider(position, m_DayDuration, 0f, 1000f);
				m_TimeIncrement = 1f / (float)m_DayDuration;
			}
		}

		private void Update()
		{
			if (!m_StopTime)
			{
				NormalizedTime += Time.deltaTime * m_TimeIncrement;
			}
			m_CurrentHour = (int)(NormalizedTime * 24f);
			RenderSettings.ambientIntensity = Mathf.Clamp01(m_Sun.intensity);
			RenderSettings.fogDensity = m_FogIntensity.Evaluate(NormalizedTime);
			RenderSettings.fogColor = m_FogColor.Evaluate(NormalizedTime);
			RenderSettings.ambientLight = m_AmbientLightColor.Evaluate(NormalizedTime);
			m_SunTransform.rotation = Quaternion.Euler(m_RotationAxis * (NormalizedTime * 360f - 90f));
			m_Sun.intensity = m_SunIntensity.Evaluate(NormalizedTime);
			m_Sun.color = m_SunColor.Evaluate(NormalizedTime);
			m_Sun.enabled = m_Sun.intensity > 0f;
			m_MoonTransform.rotation = Quaternion.Euler(-m_RotationAxis * (NormalizedTime * 360f - 90f));
			m_Moon.intensity = m_MoonIntensity.Evaluate(NormalizedTime);
			m_Moon.color = m_MoonColor.Evaluate(NormalizedTime);
			m_Moon.enabled = m_Moon.intensity > 0f;
			if ((bool)m_Skybox)
			{
				m_Skybox.SetFloat("_Blend", m_SkyboxBlend.Evaluate(m_NormalizedTime));
			}
			ET.TimeOfDay internalState = m_InternalState;
			m_InternalState = ((!NormalizedTime.IsInRangeLimitsExcluded(0.25f, 0.75f)) ? ET.TimeOfDay.Night : ET.TimeOfDay.Day);
			if (internalState != m_InternalState)
			{
				State.Set(m_InternalState);
			}
			GameController.NormalizedTime = NormalizedTime;
			if (Time.time >= timeTemp + (float)UpdateInterval)
			{
				timeTemp = Time.time;
				PlayerPrefs.SetFloat("DAY_TIME", m_NormalizedTime);
			}
		}

		private void OnValidate()
		{
			AccommodateEditorChanges();
			m_SunTransform = m_Sun.transform;
			m_MoonTransform = m_Moon.transform;
			Update();
		}

		private void AccommodateEditorChanges()
		{
			m_TimeIncrement = 1f / (float)m_DayDuration;
			m_NormalizedTime = (float)m_CurrentHour / 24f;
			RenderSettings.fogMode = m_FogMode;
		}
	}
}
