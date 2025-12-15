using System;
using UnityEngine;

namespace UltimateSurvival
{
	public class ItemProperty
	{
		public enum Type
		{
			None = 0,
			Bool = 1,
			Int = 2,
			IntRange = 3,
			RandomInt = 4,
			Float = 5,
			FloatRange = 6,
			RandomFloat = 7,
			String = 8,
			Sound = 9
		}

		[Serializable]
		public struct Int
		{
			[SerializeField]
			private int m_Current;

			[SerializeField]
			private int m_Default;

			public int Current
			{
				get
				{
					return m_Current;
				}
				set
				{
					m_Current = value;
				}
			}

			public int Default
			{
				get
				{
					return m_Default;
				}
			}

			public float Ratio
			{
				get
				{
					return (float)m_Current / (float)m_Default;
				}
			}

			public override string ToString()
			{
				return m_Current.ToString();
			}
		}

		[Serializable]
		public struct IntRange
		{
			[SerializeField]
			private int m_Current;

			[SerializeField]
			private int m_Min;

			[SerializeField]
			private int m_Max;

			public int Current
			{
				get
				{
					return m_Current;
				}
				set
				{
					m_Current = Mathf.Clamp(value, m_Min, m_Max);
				}
			}

			public float Ratio
			{
				get
				{
					return (float)m_Current / (float)m_Max;
				}
			}

			public int Min
			{
				get
				{
					return m_Min;
				}
				set
				{
					m_Min = value;
				}
			}

			public int Max
			{
				get
				{
					return m_Max;
				}
				set
				{
					m_Max = value;
				}
			}

			public override string ToString()
			{
				return string.Format("{0} / {1}", Current, Max);
			}
		}

		[Serializable]
		public struct RandomInt
		{
			[SerializeField]
			private int m_Min;

			[SerializeField]
			private int m_Max;

			public int RandomValue
			{
				get
				{
					return UnityEngine.Random.Range(m_Min, m_Max);
				}
			}

			public override string ToString()
			{
				return string.Format("{0} - {1}", m_Min, m_Max);
			}
		}

		[Serializable]
		public struct Float
		{
			[SerializeField]
			private float m_Current;

			[SerializeField]
			private float m_Default;

			public float Current
			{
				get
				{
					return m_Current;
				}
				set
				{
					m_Current = value;
				}
			}

			public float Default
			{
				get
				{
					return m_Default;
				}
			}

			public float Ratio
			{
				get
				{
					return m_Current / m_Default;
				}
			}

			public override string ToString()
			{
				return m_Current.ToString();
			}
		}

		[Serializable]
		public struct FloatRange
		{
			[SerializeField]
			private float m_Current;

			[SerializeField]
			private float m_Min;

			[SerializeField]
			private float m_Max;

			public float Current
			{
				get
				{
					return m_Current;
				}
				set
				{
					m_Current = Mathf.Clamp(value, m_Min, m_Max);
				}
			}

			public float Ratio
			{
				get
				{
					return m_Current / m_Max;
				}
			}

			public float Min
			{
				get
				{
					return m_Min;
				}
				set
				{
					m_Min = value;
				}
			}

			public float Max
			{
				get
				{
					return m_Max;
				}
				set
				{
					m_Max = value;
				}
			}

			public override string ToString()
			{
				return string.Format("{0} / {1}", Current, Max);
			}
		}

		[Serializable]
		public struct RandomFloat
		{
			[SerializeField]
			private float m_Min;

			[SerializeField]
			private float m_Max;

			public float RandomValue
			{
				get
				{
					return UnityEngine.Random.Range(m_Min, m_Max);
				}
			}

			public override string ToString()
			{
				return string.Format("{0} - {1}", m_Min, m_Max);
			}
		}

		[Serializable]
		public struct Definition
		{
			[SerializeField]
			private string m_Name;

			[SerializeField]
			private Type m_Type;

			public string Name
			{
				get
				{
					return m_Name;
				}
			}

			public Type Type
			{
				get
				{
					return m_Type;
				}
			}
		}

		[Serializable]
		public class Value
		{
			public Message<Value> Changed = new Message<Value>();

			[SerializeField]
			private string m_Name;

			[SerializeField]
			private Type m_Type;

			[SerializeField]
			private bool m_Bool;

			[SerializeField]
			private Int m_Int;

			[SerializeField]
			private IntRange m_IntRange;

			[SerializeField]
			private RandomInt m_RandomInt;

			[SerializeField]
			private Float m_Float;

			[SerializeField]
			private FloatRange m_FloatRange;

			[SerializeField]
			private RandomFloat m_RandomFloat;

			[SerializeField]
			private string m_String;

			[SerializeField]
			private AudioClip m_Sound;

			public string Name
			{
				get
				{
					return m_Name;
				}
			}

			public Type Type { get; private set; }

			public bool Bool
			{
				get
				{
					return m_Bool;
				}
			}

			public Int Int
			{
				get
				{
					return m_Int;
				}
			}

			public IntRange IntRange
			{
				get
				{
					return m_IntRange;
				}
			}

			public RandomInt RandomInt
			{
				get
				{
					return m_RandomInt;
				}
			}

			public Float Float
			{
				get
				{
					return m_Float;
				}
			}

			public FloatRange FloatRange
			{
				get
				{
					return m_FloatRange;
				}
			}

			public RandomFloat RandomFloat
			{
				get
				{
					return m_RandomFloat;
				}
			}

			public string String
			{
				get
				{
					return m_String;
				}
			}

			public AudioClip Sound
			{
				get
				{
					return m_Sound;
				}
			}

			public Value GetClone()
			{
				return (Value)MemberwiseClone();
			}

			public void SetValue(Type type, object value)
			{
				switch (type)
				{
				case Type.Bool:
					m_Bool = (bool)value;
					break;
				case Type.Int:
					m_Int = (Int)value;
					break;
				case Type.Float:
					m_Float = (Float)value;
					break;
				case Type.FloatRange:
					m_FloatRange = (FloatRange)value;
					break;
				case Type.IntRange:
					m_IntRange = (IntRange)value;
					break;
				case Type.RandomFloat:
					m_RandomFloat = (RandomFloat)value;
					break;
				case Type.RandomInt:
					m_RandomInt = (RandomInt)value;
					break;
				case Type.String:
					m_String = (string)value;
					break;
				case Type.None:
					return;
				}
				Changed.Send(this);
			}

			public override string ToString()
			{
				if (m_Type == Type.Bool)
				{
					return m_Bool.ToString();
				}
				if (m_Type == Type.Int)
				{
					return m_Int.ToString();
				}
				if (m_Type == Type.Float)
				{
					return m_Float.ToString();
				}
				if (m_Type == Type.FloatRange)
				{
					return m_FloatRange.ToString();
				}
				if (m_Type == Type.IntRange)
				{
					return m_IntRange.ToString();
				}
				if (m_Type == Type.RandomFloat)
				{
					return m_RandomFloat.ToString();
				}
				if (m_Type == Type.RandomInt)
				{
					return m_RandomInt.ToString();
				}
				if (m_Type == Type.String)
				{
					return m_String;
				}
				return m_Name;
			}
		}
	}
}
