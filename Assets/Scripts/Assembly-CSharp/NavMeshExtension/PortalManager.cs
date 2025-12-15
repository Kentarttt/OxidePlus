using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshExtension
{
	public class PortalManager : MonoBehaviour
	{
		[Serializable]
		public class PortalPath
		{
			[Serializable]
			public class PortalNode
			{
				private static PortalPath path;

				public Vector3 start;

				public Vector3? target;

				public float length;

				public List<PortalNode> childs = new List<PortalNode>();

				public PortalNode(PortalPath instance, Vector3 start)
				{
					path = instance;
					this.start = start;
					NavMeshPath navMeshPath = new NavMeshPath();
					if (NavMesh.CalculatePath(start, instance.final, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
					{
						target = instance.final;
					}
				}

				public PortalNode(Vector3 tar, List<Transform> nodes, int index)
				{
					start = tar;
					List<Transform> list = new List<Transform>(nodes);
					Transform transform = list[index];
					target = FindNearest(transform, start);
					if (target.HasValue)
					{
						Vector3 complement = GetComplement(transform, target.Value);
						list.Remove(transform);
						childs.Add(new PortalNode(path, complement));
						for (int i = 0; i < list.Count; i++)
						{
							childs.Add(new PortalNode(complement, list, i));
						}
					}
				}

				public string PrintPath()
				{
					string text = string.Concat(" ", start, "->", target);
					for (int i = 0; i < childs.Count; i++)
					{
						text += childs[i].PrintPath();
					}
					return text;
				}

				public void CalculateLength(float parLength)
				{
					NavMeshPath navMeshPath = new NavMeshPath();
					float num = float.PositiveInfinity;
					if (target.HasValue && NavMesh.CalculatePath(start, target.Value, -1, navMeshPath))
					{
						num = parLength + PathLength(navMeshPath);
					}
					length = num;
					for (int i = 0; i < childs.Count; i++)
					{
						childs[i].CalculateLength(length);
					}
				}

				public float GetShortestLength()
				{
					if (childs.Count == 0)
					{
						return length;
					}
					float num = float.PositiveInfinity;
					for (int i = 0; i < childs.Count; i++)
					{
						float shortestLength = childs[i].GetShortestLength();
						if (shortestLength < num)
						{
							num = shortestLength;
						}
					}
					return num;
				}

				public List<Vector3> GetShortestPath()
				{
					List<Vector3> list = new List<Vector3>();
					if (target.HasValue)
					{
						list.Add(start);
						list.Add(target.Value);
					}
					if (!target.HasValue || childs.Count == 0)
					{
						return list;
					}
					float num = float.PositiveInfinity;
					int index = 0;
					for (int i = 0; i < childs.Count; i++)
					{
						float shortestLength = childs[i].GetShortestLength();
						if (shortestLength < num)
						{
							num = shortestLength;
							index = i;
						}
					}
					list.AddRange(childs[index].GetShortestPath());
					return list;
				}
			}

			public Vector3 pos;

			public Vector3 final;

			public Vector3[] GetShortestPath()
			{
				List<Transform> list = new List<Transform>();
				foreach (Transform key in portals.Keys)
				{
					list.Add(key);
				}
				List<PortalNode> list2 = new List<PortalNode>();
				list2.Add(new PortalNode(this, pos));
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(new PortalNode(pos, list, i));
				}
				for (int j = 0; j < list2.Count; j++)
				{
					list2[j].CalculateLength(0f);
				}
				float num = float.PositiveInfinity;
				int index = 0;
				for (int k = 0; k < list2.Count; k++)
				{
					float shortestLength = list2[k].GetShortestLength();
					if (shortestLength < num)
					{
						num = shortestLength;
						index = k;
					}
				}
				return list2[index].GetShortestPath().ToArray();
			}
		}

		private static PortalManager instance;

		public static readonly Dictionary<Transform, List<Transform>> portals = new Dictionary<Transform, List<Transform>>();

		private void Awake()
		{
			instance = this;
			Init();
		}

		private void OnDestroy()
		{
			portals.Clear();
		}

		public static void Init()
		{
			foreach (Transform item2 in instance.transform)
			{
				if (portals.ContainsKey(item2))
				{
					continue;
				}
				portals.Add(item2, new List<Transform>());
				foreach (Transform item3 in item2)
				{
					portals[item2].Add(item3);
				}
			}
		}

		private static Vector3 GetComplement(Transform parent, Vector3 portal)
		{
			if (portals[parent][0].position == portal)
			{
				return portals[parent][1].position;
			}
			return portals[parent][0].position;
		}

		private static Vector3? FindNearest(Transform parent, Vector3 pos)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			Vector3? result = null;
			float num = float.PositiveInfinity;
			if (!portals.ContainsKey(parent))
			{
				return null;
			}
			for (int i = 0; i < portals[parent].Count; i++)
			{
				Vector3 position = portals[parent][i].position;
				float num2 = float.PositiveInfinity;
				if (NavMesh.CalculatePath(pos, position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					num2 = PathLength(navMeshPath);
				}
				if (num2 < num)
				{
					num = num2;
					result = position;
				}
			}
			return result;
		}

		private static float PathLength(NavMeshPath path)
		{
			if (path.corners.Length < 2)
			{
				return 0f;
			}
			Vector3 a = path.corners[0];
			float num = 0f;
			for (int i = 1; i < path.corners.Length; i++)
			{
				Vector3 vector = path.corners[i];
				num += Vector3.Distance(a, vector);
				a = vector;
			}
			return num;
		}

		public static Vector3[] GetPath(Vector3 start, Vector3 target)
		{
			PortalPath portalPath = new PortalPath();
			portalPath.pos = start;
			portalPath.final = target;
			return portalPath.GetShortestPath();
		}
	}
}
