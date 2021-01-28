using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Show on screen the number of pipe the player go through
/// </summary>
public class scoreManager : MonoBehaviour
{
	public static int score = 0;
	private Text txt;

	// Start is called before the first frame update
	private void Start()
	{
		txt = GetComponent<Text>();
		score = 0;
	}

	// Update is called once per frame
	private void Update()
	{
		txt.text = score.ToString();
	}
}