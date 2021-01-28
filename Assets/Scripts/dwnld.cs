using UnityEngine;
using UnityEngine.SceneManagement;

public class dwnld : MonoBehaviour
{
	// called zero
	private void Awake()
	{
		Debug.Log("Awake");
	}

	// called first
	private void OnEnable()
	{
		//GameObject obj = GameObject.FindGameObjectWithTag("CloudHandler");
		//obj.GetComponent<CloudHandler>().Init();
		//Debug.Log("OnEnable called");
		//SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// called second
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("OnSceneLoaded: " + scene.name);
		Debug.Log(mode);
	}

	// called third
	private void Start()
	{
		Debug.Log("Start");
	}

	// called when the game is terminated
	private void OnDisable()
	{
		Debug.Log("OnDisable");
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}