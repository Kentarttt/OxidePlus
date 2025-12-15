using System;
using UnityEngine;

[Serializable]
public class UVScroller_simple : MonoBehaviour
{
	public float scrollSpeed;

	public UVScroller_simple()
	{
		scrollSpeed = 0.1f;
	}

	public virtual void Update()
	{
		float num = Time.time * scrollSpeed;
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(num / 3f, num / -3f));
	}

	public virtual void Main()
	{
	}
}
