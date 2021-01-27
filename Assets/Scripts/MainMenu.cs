using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public bool isConnected = false;
	public LogInController logInController;

	[Header("Canvas")]
	public GameObject connectionCanvas;
	public GameObject mainMenuCanvas;
	public GameObject logIconCanvas;

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
