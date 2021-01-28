using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle UI element of the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{
	public bool isConnected = false;

	[Header("Input field")]
	public TMP_InputField EmailInput;
	public TMP_InputField PwdInput;

	[Header("Canvas")]
	public GameObject connectionCanvas;

	public GameObject mainMenuCanvas;
	public GameObject logIconCanvas;
	public GameObject logOutCanvas;

	public void GoPlay()
	{
		SceneManager.LoadScene(1);
	}
}