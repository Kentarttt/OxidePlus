using UnityEngine;

namespace UltimateSurvival
{
	public class SurfaceDatabase : ScriptableSingleton<SurfaceDatabase>
	{
		[SerializeField]
		private SurfaceData m_DefaultSurface;

		[SerializeField]
		private SurfaceData[] m_Surfaces;

		public SurfaceData GetSurfaceData(Texture texture)
		{
			for (int i = 0; i < m_Surfaces.Length; i++)
			{
				if (m_Surfaces[i].HasTexture(texture))
				{
					return m_Surfaces[i];
				}
			}
			return m_DefaultSurface;
		}

		public SurfaceData GetSurfaceData(Ray ray, float maxDistance, LayerMask mask)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, maxDistance, mask, QueryTriggerInteraction.Ignore))
			{
				return GetSurfaceData(hitInfo.collider, hitInfo.point, hitInfo.triangleIndex);
			}
			return null;
		}

		public SurfaceData GetSurfaceData(RaycastHit hitInfo)
		{
			return GetSurfaceData(hitInfo.collider, hitInfo.point, hitInfo.triangleIndex);
		}

		public SurfaceData GetSurfaceData(Collider collider, Vector3 position, int triangleIndex)
		{
			Texture texture = null;
			if (collider.GetType() == typeof(TerrainCollider))
			{
				Terrain component = collider.GetComponent<Terrain>();
				TerrainData terrainData = component.terrainData;
				float[] terrainTextureMix = GetTerrainTextureMix(position, terrainData, component.GetPosition());
				int terrainTextureIndex = GetTerrainTextureIndex(position, terrainTextureMix);
				texture = terrainData.splatPrototypes[terrainTextureIndex].texture;
			}
			else
			{
				texture = GetMeshTexture(collider, triangleIndex);
			}
			if ((bool)texture)
			{
				for (int i = 0; i < m_Surfaces.Length; i++)
				{
					if (m_Surfaces[i].HasTexture(texture))
					{
						return m_Surfaces[i];
					}
				}
				return m_DefaultSurface;
			}
			return m_DefaultSurface;
		}

		private Texture GetMeshTexture(Collider collider, int triangleIndex)
		{
			SurfaceIdentity component = collider.GetComponent<SurfaceIdentity>();
			if ((bool)component)
			{
				return component.Texture;
			}
			Renderer component2 = collider.GetComponent<Renderer>();
			MeshCollider meshCollider = collider as MeshCollider;
			if (!component2 || !component2.sharedMaterial || !component2.sharedMaterial.mainTexture)
			{
				return null;
			}
			if (!meshCollider || meshCollider.convex)
			{
				return component2.material.mainTexture;
			}
			Mesh sharedMesh = meshCollider.sharedMesh;
			int num = -1;
			int num2 = sharedMesh.triangles[triangleIndex * 3];
			int num3 = sharedMesh.triangles[triangleIndex * 3 + 1];
			int num4 = sharedMesh.triangles[triangleIndex * 3 + 2];
			for (int i = 0; i < sharedMesh.subMeshCount; i++)
			{
				int[] triangles = sharedMesh.GetTriangles(i);
				for (int j = 0; j < triangles.Length; j += 3)
				{
					if (triangles[j] == num2 && triangles[j + 1] == num3 && triangles[j + 2] == num4)
					{
						num = i;
						break;
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			return component2.materials[num].mainTexture;
		}

		private float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
		{
			int x = (int)((worldPos.x - terrainPos.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
			int y = (int)((worldPos.z - terrainPos.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
			float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
			float[] array = new float[alphamaps.GetUpperBound(2) + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = alphamaps[0, 0, i];
			}
			return array;
		}

		private int GetTerrainTextureIndex(Vector3 worldPos, float[] textureMix)
		{
			float num = 0f;
			int result = 0;
			for (int i = 0; i < textureMix.Length; i++)
			{
				if (textureMix[i] > num)
				{
					result = i;
					num = textureMix[i];
				}
			}
			return result;
		}
	}
}
