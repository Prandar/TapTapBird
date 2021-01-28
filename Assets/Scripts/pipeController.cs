using UnityEngine;

/// <summary>
/// Move the pipes from right to left
/// </summary>
public class pipeController : MonoBehaviour
{
	public float speed;

	// Update is called once per frame
	private void Update()
	{
		transform.position += Vector3.left * speed * Time.deltaTime;
	}
}