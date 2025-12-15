using System;
using UnityEngine;

[Serializable]
public class WaterShaderHelper : MonoBehaviour
{
	public Transform lightDir;

	public virtual void Update()
	{
		Material material = GetComponent<Renderer>().material;
		if ((bool)lightDir)
		{
			material.SetVector("_WorldLightDir", lightDir.forward);
		}
		else
		{
			material.SetVector("_WorldLightDir", new Vector3(0.7f, 0.7f, 0f));
		}
	}

	public virtual void Main()
	{
	}
}
