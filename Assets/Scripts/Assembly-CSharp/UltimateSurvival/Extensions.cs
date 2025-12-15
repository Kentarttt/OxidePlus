using System.Collections.Generic;
using UltimateSurvival.AI;
using UnityEngine;

namespace UltimateSurvival
{
	public static class Extensions
	{
		public static Transform FindDeepChild(this Transform parent, string childName)
		{
			Transform transform = parent.Find(childName);
			if ((bool)transform)
			{
				return transform;
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				transform = parent.GetChild(i).FindDeepChild(childName);
				if ((bool)transform)
				{
					return transform;
				}
			}
			return null;
		}

		public static bool IndexIsValid<T>(this List<T> list, int index)
		{
			return index >= 0 && index < list.Count;
		}

		public static List<T> CopyOther<T>(this List<T> list, List<T> toCopy)
		{
			if (toCopy == null || toCopy.Count == 0)
			{
				return null;
			}
			list = new List<T>();
			for (int i = 0; i < toCopy.Count; i++)
			{
				list.Add(toCopy[i]);
			}
			return list;
		}

		public static bool IsInRangeLimitsExcluded(this float f, float l1, float l2)
		{
			return f > l1 && f < l2;
		}

		public static bool IsInRangeLimitsIncluded(this float f, float l1, float l2)
		{
			return f >= l1 && f <= l2;
		}

		public static string GetDisplayInfoForElement(this StateData data, string key)
		{
			string result = string.Empty;
			object value = null;
			if (data.m_Dictionary.TryGetValue(key, out value))
			{
				result = "Key: " + key + " -  Value: " + value;
			}
			else
			{
				Debug.LogError("Could not print element with key " + key);
			}
			return result;
		}

		public static string GetDisplayInfoForAll(this StateData data, string sepparator = " - ")
		{
			string text = string.Empty;
			foreach (KeyValuePair<string, object> item in data.m_Dictionary)
			{
				text = text + sepparator + data.GetDisplayInfoForElement(item.Key);
			}
			return text;
		}
	}
}
