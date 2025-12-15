using UnityEngine;
using UnityEngine.UI;

public class LoadingSpinner : MonoBehaviour
{
	public Image placeholder;

	public Sprite[] frames;

	public int framesPerSecond = 8;

	private int count;

	private void Start()
	{
		count = frames.Length;
	}

	private void Update()
	{
		int num = (int)(Time.time * (float)framesPerSecond) % count;
		placeholder.sprite = frames[num];
	}
}
