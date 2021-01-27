using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
	[Header("Scene index")]
	public int menuScene = 0;
	public int gameScene = 1;
	public int ShopScene = 2;

	[Header("Canvas")]
	public GameObject getReadyCanvas;

	public GameObject scoreCanvas;
	public GameObject gameOverCanvas;
	public GameObject highScoreOffline;
	public TextMeshProUGUI actualScore;
	public TextMeshProUGUI highScore;

	[Header("Controller")]
	public playerController player;
	private CloudHandler ch;

	private bool isPlaying;

	//[Header("Other Manager")]
	//public scoreManager scoreManager;

	// Start is called before the first frame update
	private void Start()
	{
		Time.timeScale = 0;
		getReadyCanvas.SetActive(true);
		scoreCanvas.SetActive(false);
		highScoreOffline.SetActive(false);
		gameOverCanvas.SetActive(false);

		GameObject[] objs = GameObject.FindGameObjectsWithTag("CloudHandler");
		ch = objs[0].GetComponent<CloudHandler>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (!isPlaying && Input.GetMouseButtonDown(0))
		{
			isPlaying = true;
			getReadyCanvas.SetActive(false);
			scoreCanvas.SetActive(true);
			Time.timeScale = 1;
			player.rb.velocity = Vector2.up * player.velocity;
		}
	}

	public void GameOver()
	{
		gameOverCanvas.SetActive(true);
		Time.timeScale = 0;
		ch.InvokeCreateKvStoreKeyUserBatch();
	}

	public void GoMenu()
	{
		SceneManager.LoadScene(menuScene);
	}

	public void Replay()
	{
		PlayerPrefs.SetInt("highScore", scoreManager.score);
		SceneManager.LoadScene(gameScene);
	}

	public void ShowOffLineHighScore()
	{
		int newScore = scoreManager.score;
		actualScore.text = newScore.ToString();
		if (newScore > 9)
		{
			//set new hs
			PlayerPrefs.SetInt("highScore", scoreManager.score);
		}
		highScore.text = newScore.ToString();
		scoreCanvas.SetActive(false);
		gameOverCanvas.SetActive(false);
		highScoreOffline.SetActive(true);
	}

	public void FromHSOfflineToGameOver()
	{
		gameOverCanvas.SetActive(true);
		highScoreOffline.SetActive(false);
	}
}