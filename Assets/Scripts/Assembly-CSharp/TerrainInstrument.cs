using UnityEngine;

public class TerrainInstrument : MonoBehaviour
{
	[ContextMenu("Set Trees Colors")]
	private void SetTrees()
	{
		TreeInstance[] treeInstances = Terrain.activeTerrain.terrainData.treeInstances;
		int num = Terrain.activeTerrain.terrainData.treeInstances.Length;
		for (int i = 0; i < num; i++)
		{
			float num2 = Random.Range(1, 1);
			treeInstances[i].color = new Color(num2, num2, num2);
			treeInstances[i].heightScale = 1f;
			treeInstances[i].widthScale = 1f;
		}
		Terrain.activeTerrain.terrainData.treeInstances = treeInstances;
	}

	[ContextMenu("Save Trees Coords")]
	private void SaveTreesCoords()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("trees");
		string[] array = textAsset.text.Split("|"[0]);
		TreeInstance[] array2 = new TreeInstance[array.Length - 1];
		for (int i = 0; i < array.Length - 1; i++)
		{
			string[] array3 = array[i].Split(","[0]);
			TreeInstance treeInstance = default(TreeInstance);
			treeInstance.prototypeIndex = 0;
			treeInstance.heightScale = 1f;
			treeInstance.lightmapColor = Color.white;
			treeInstance.color = Color.white;
			treeInstance.position = new Vector3(float.Parse(array3[0]), float.Parse(array3[1]), float.Parse(array3[2]));
			treeInstance.widthScale = 1f;
			treeInstance.rotation = Random.Range(0, 5);
			array2[i] = treeInstance;
		}
		Terrain.activeTerrain.terrainData.treeInstances = array2;
		Debug.Log("Completed.");
	}

	[ContextMenu("Trees Count")]
	private void TreesCount()
	{
		Debug.Log(base.transform.childCount);
	}
}
