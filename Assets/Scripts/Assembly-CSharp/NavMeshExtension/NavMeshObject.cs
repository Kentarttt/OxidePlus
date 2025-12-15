using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NavMeshExtension
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class NavMeshObject : MonoBehaviour
	{
		[Serializable]
		public class SubPoints
		{
			public List<int> list = new List<int>();
		}

		public bool autoSplit;

		public int splitAt = 4;

		public float yOffset = 0.015f;

		[HideInInspector]
		public List<Vector3> list = new List<Vector3>();

		[HideInInspector]
		public List<int> current = new List<int>();

		[HideInInspector]
		public List<SubPoints> subPoints = new List<SubPoints>();

		[HideInInspector]
		public Mesh subMesh;

		public void Combine()
		{
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
			MeshFilter meshFilter = componentsInChildren[0];
			List<CombineInstance> list = new List<CombineInstance>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (!(componentsInChildren[j].sharedMesh == null))
				{
					CombineInstance item = default(CombineInstance);
					item.mesh = componentsInChildren[j].sharedMesh;
					item.transform = componentsInChildren[j].transform.localToWorldMatrix;
					list.Add(item);
				}
			}
			string text = "NavMesh";
			if (meshFilter.sharedMesh != null)
			{
				text = meshFilter.sharedMesh.name;
			}
			meshFilter.sharedMesh = new Mesh();
			meshFilter.sharedMesh.name = text;
			meshFilter.sharedMesh.CombineMeshes(list.ToArray());
			current.Clear();
			List<Vector3> list2 = new List<Vector3>(meshFilter.sharedMesh.vertices);
			List<int> triangles = new List<int>(meshFilter.sharedMesh.triangles);
			for (int k = 0; k < list2.Count; k++)
			{
				list2[k] = base.transform.InverseTransformPoint(list2[k]);
			}
			List<int> list3 = new List<int>();
			List<Vector3> duplicates = (from x in list2
				group x by x into x
				where x.Count() > 1
				select x.Key).ToList();
			for (int i = 0; i < duplicates.Count; i++)
			{
				List<int> list4 = (from a in list2.Select((Vector3 value, int index) => new { value, index })
					where object.Equals(a.value, duplicates[i])
					select a.index).ToList();
				int value2 = list4[0];
				list4.RemoveAt(0);
				for (int l = 0; l < list4.Count; l++)
				{
					int dupIndex = list4[l];
					List<int> list5 = (from v in Enumerable.Range(0, triangles.Count)
						where triangles[v] == dupIndex
						select v).ToList();
					for (int m = 0; m < list5.Count; m++)
					{
						triangles[list5[m]] = value2;
					}
					list3.Add(dupIndex);
				}
			}
			list3 = list3.OrderByDescending((int x) => x).ToList();
			for (int n = 0; n < list3.Count; n++)
			{
				int num = list3[n];
				list2.RemoveAt(num);
				for (int num2 = num; num2 < triangles.Count; num2++)
				{
					if (triangles[num2] >= num)
					{
						triangles[num2] -= 1;
					}
				}
			}
			meshFilter.sharedMesh.triangles = triangles.ToArray();
			meshFilter.sharedMesh.vertices = list2.ToArray();
			meshFilter.sharedMesh.RecalculateNormals();
			meshFilter.sharedMesh.RecalculateBounds();
			Mesh sharedMesh = meshFilter.sharedMesh;
		}

		public GameObject CreateSubMesh()
		{
			subPoints.Add(new SubPoints());
			GameObject gameObject = new GameObject("New SubMesh");
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = Vector3.zero;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			meshRenderer.enabled = GetComponent<Renderer>().enabled;
			meshFilter.mesh = (subMesh = new Mesh());
			subMesh.name = "SubMesh";
			current.Clear();
			return gameObject;
		}

		public void UpdateMesh(Vector3[] verts)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = base.transform.InverseTransformPoint(verts[i]);
			}
			component.sharedMesh.vertices = verts;
		}

		public void AddPoint(Vector3 point)
		{
			point += new Vector3(0f, yOffset, 0f);
			if (list.Count == 0)
			{
				base.transform.position = point;
			}
			list.Add(base.transform.InverseTransformPoint(point));
			int item = list.Count - 1;
			current.Add(item);
			subPoints[subPoints.Count - 1].list.Add(item);
		}

		public void AddPoint(int point)
		{
			current.Add(point);
			subPoints[subPoints.Count - 1].list.Add(point);
		}

		public void CreateMesh()
		{
			if ((bool)subMesh)
			{
				subMesh.Clear();
			}
			MeshFilter meshFilter = null;
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].sharedMesh == subMesh)
				{
					meshFilter = componentsInChildren[i];
					break;
				}
			}
			Vector3[] array = new Vector3[current.Count];
			for (int j = 0; j < current.Count; j++)
			{
				array[j] = list[current[j]];
			}
			if (!meshFilter || array.Length < 3)
			{
				return;
			}
			Vector2[] array2 = new Vector2[array.Length];
			for (int k = 0; k < array.Length; k++)
			{
				if (k % 2 == 0)
				{
					array2[k] = new Vector2(0f, 0f);
				}
				else
				{
					array2[k] = new Vector2(1f, 1f);
				}
			}
			subMesh.vertices = array;
			subMesh.uv = array2;
			subMesh.triangles = RecalculateTriangles(null);
			subMesh.RecalculateNormals();
			subMesh.RecalculateBounds();
			meshFilter.mesh = subMesh;
		}

		public int[] RecalculateTriangles(List<int> list)
		{
			int num = ((list != null) ? (list.Count - 2) : (current.Count - 2));
			int[] array = new int[3 * num * 2];
			int num2 = ((list != null) ? list[0] : 0);
			int num3 = ((list == null) ? 1 : list[1]);
			int num4 = ((list != null) ? list[2] : 2);
			for (int i = 0; i < array.Length / 2; i += 3)
			{
				array[i] = num2;
				array[i + 1] = num3;
				array[i + 2] = num4;
				num3++;
				num4++;
			}
			for (int j = array.Length / 2; j < array.Length; j += 3)
			{
				int num5 = (j - array.Length / 2) * 2;
				array[j] = num2;
				array[j + 1] = array[j - num5 - 1];
				array[j + 2] = array[j - num5 - 2];
			}
			return array;
		}
	}
}
