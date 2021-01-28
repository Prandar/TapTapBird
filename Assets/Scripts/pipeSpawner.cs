using UnityEngine;

/// <summary>
/// Spawn pipe prefab every maxTime seconds with a random y position between height and -height
/// </summary>
public class pipeSpawner : MonoBehaviour
{
	public float maxTime = 1;
	private float timer = 0;
	public GameObject pipe;
	public float height;

	// Update is called once per frame
	private void Update()
	{
		if (timer > maxTime)
		{
			GameObject newPipe = Instantiate(pipe);
			newPipe.transform.position = transform.position + new Vector3(0, Random.Range(-height, height), 0);
			Destroy(newPipe, 15);
			timer = 0;
		}
		timer += Time.deltaTime;
	}
}