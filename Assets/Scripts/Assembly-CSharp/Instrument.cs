using UnityEngine;

public class Instrument : MonoBehaviour
{
	public GameObject treeCollider;

	[ContextMenu("Set LODs")]
	private void SetLODs()
	{
		Transform[] array = new Transform[base.transform.childCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = base.transform.GetChild(i);
		}
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			GameObject gameObject = new GameObject(transform.name);
			gameObject.transform.parent = base.transform;
			transform.parent = gameObject.transform;
			LODGroup lODGroup = gameObject.AddComponent<LODGroup>();
			LOD[] array3 = new LOD[1];
			Renderer[] renderers = new Renderer[1] { transform.gameObject.GetComponent<Renderer>() };
			array3[0] = new LOD(0.3f, renderers);
			lODGroup.SetLODs(array3);
			lODGroup.RecalculateBounds();
		}
	}

	[ContextMenu("Set Titles")]
	private void SetTitles()
	{
		Transform[] array = new Transform[base.transform.childCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = base.transform.GetChild(i);
		}
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			string text = transform.GetChild(0).name;
			transform.GetChild(0).name = ((!text.Equals("Rock7")) ? "Rock_Metal" : "Rock_Normal");
		}
	}

	[ContextMenu("Place trees colliders")]
	private void PlaceColliders()
	{
		TreeInstance[] treeInstances = Terrain.activeTerrain.terrainData.treeInstances;
		for (int i = 0; i < treeInstances.Length; i++)
		{
			TreeInstance treeInstance = treeInstances[i];
			GameObject gameObject = Object.Instantiate(treeCollider, new Vector3(treeInstance.position.x * 2000f, treeInstance.position.y * 100f, treeInstance.position.z * 2000f), Quaternion.identity);
			gameObject.transform.parent = base.transform;
		}
		Debug.Log("Completed");
	}
}
