using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public static class ScriptUtilities
	{
		public static List<Transform> GetTransformsByTag(string tag)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag(tag);
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i].transform);
			}
			return list;
		}

		public static bool GetTransformsPositionsByTag(string tag, out List<Vector3> posS)
		{
			bool result = false;
			GameObject[] array = GameObject.FindGameObjectsWithTag(tag);
			posS = new List<Vector3>();
			for (int i = 0; i < array.Length; i++)
			{
				posS.Add(array[i].transform.position);
			}
			if (posS.Count > 0)
			{
				result = true;
			}
			return result;
		}

		public static List<Vector3> GetRandomPositionsAroundTransform(Transform transform, int amount = 5, int radius = 5, float distanceBtwPoints = 5f)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < amount; i++)
			{
				Vector3 vector = Random.insideUnitSphere * radius + transform.position;
				vector.y = transform.position.y;
				if (list.IndexIsValid(i - 1))
				{
					float num = Vector3.Distance(vector, list[i - 1]);
					if (num < distanceBtwPoints)
					{
						vector += new Vector3(num, 0f, num);
					}
				}
				list.Add(vector);
			}
			return list;
		}
	}
}
