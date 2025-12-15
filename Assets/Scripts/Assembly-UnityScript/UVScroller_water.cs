using System;
using UnityEngine;

[Serializable]
public class UVScroller_water : MonoBehaviour
{
	public float scrollSpeed;

	public UVScroller_water()
	{
		scrollSpeed = 0.1f;
	}

	public virtual void Update()
	{
		float num = Time.time * scrollSpeed;
		Material material = GetComponent<Renderer>().material;
		material.SetTextureOffset("_MainTex", new Vector2(num * 0.5f, num * 1f));
		material.SetTextureOffset("_HeightTex", new Vector2(num / 2f, num));
		material.SetTextureOffset("_FoamTex", new Vector2(num / 4f, num * 1f));
	}

	public virtual void Main()
	{
	}
}
