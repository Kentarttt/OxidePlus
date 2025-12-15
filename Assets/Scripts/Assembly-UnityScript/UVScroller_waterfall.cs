using System;
using UnityEngine;

[Serializable]
public class UVScroller_waterfall : MonoBehaviour
{
	public float scrollSpeed;

	public UVScroller_waterfall()
	{
		scrollSpeed = 0.1f;
	}

	public virtual void Update()
	{
		float num = Time.time * scrollSpeed;
		Material material = GetComponent<Renderer>().material;
		material.SetTextureOffset("_MainTex", new Vector2(num / 3f, num));
		material.SetTextureOffset("_BumpMap", new Vector2(num / 3.5f, num));
		material.SetTextureOffset("_FoamTex", new Vector2(num / 4f, num * 2f));
	}

	public virtual void Main()
	{
	}
}
