using System;
using UnityEngine;

namespace UltimateSurvival
{
	[Serializable]
	public class TreeCreator
	{
		[SerializeField]
		[Clamp(0f, 100f)]
		private int m_PrototypeIndex;

		[SerializeField]
		private MineableObject m_Prefab;

		[SerializeField]
		private Vector3 m_PositionOffset;

		[SerializeField]
		private Vector3 m_RotationOffset;

		public int PrototypeIndex
		{
			get
			{
				return m_PrototypeIndex;
			}
		}

		public MineableObject CreateTree(Terrain terrain, TreeInstance treeInstance, int treeIndex)
		{
			if (!m_Prefab)
			{
				Debug.LogError("[TreeCreator] - This tree creator has no tree prefab assigned! | Prototype Index: " + m_PrototypeIndex);
				return null;
			}
			Transform transform = null;
			if (terrain.transform.FindDeepChild("Trees") != null)
			{
				transform = terrain.transform.FindDeepChild("Trees");
			}
			else
			{
				transform = new GameObject("Trees").transform;
				transform.position = terrain.transform.position;
				transform.SetParent(terrain.transform, true);
			}
			Vector3 vector = Vector3.Scale(treeInstance.position, terrain.terrainData.size);
			MineableObject mineableObject = UnityEngine.Object.Instantiate(m_Prefab, vector + m_PositionOffset, Quaternion.Euler(m_RotationOffset), transform);
			mineableObject.Destroyed.AddListener(delegate
			{
				On_TreeDestroyed(terrain, treeInstance, treeIndex);
			});
			return mineableObject;
		}

		private void On_TreeDestroyed(Terrain terrain, TreeInstance treeInstance, int treeIndex)
		{
			treeInstance.widthScale = (treeInstance.heightScale = 0f);
			terrain.terrainData.SetTreeInstance(treeIndex, treeInstance);
		}
	}
}
