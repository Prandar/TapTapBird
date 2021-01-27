using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public bool isConnected = false;
	public LogInController logInController;

	[Header("Input field")]
	public TMP_InputField EmailInput;
	public TMP_InputField PwdInput;

	[Header("Canvas")]
	public GameObject connectionCanvas;
	public GameObject mainMenuCanvas;
	public GameObject logIconCanvas;
	public GameObject logOutCanvas;

	void Start()
	{
		if (true)
		{
			//mainMenuCanvas.SetActive(false);
		}
	}

    public void GoPlay()
	{
		SceneManager.LoadScene(1);
	}
}
