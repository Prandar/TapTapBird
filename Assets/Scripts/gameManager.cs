using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle Game Event and UI element in Game scene
/// </summary>
public class gameManager : MonoBehaviour
{
	[Header("Scene index")]
	public int menuScene = 0;
	public int gameScene = 1;
	public int ShopScene = 2;

	[Header("Canvas")]
	public GameObject highScoreOffline;
	public GameObject highScoreOnline;
	public GameObject getReadyCanvas;
	public GameObject gameOverCanvas;
	public GameObject scoreCanvas;
	public TextMeshProUGUI actualScore;
	public TextMeshProUGUI highScore;
	public GameObject scoreTxtParent;
	public GameObject scoreTxt;

	[Header("Controller")]
	public playerController player;

	private CloudHandler ch;
	private bool isPlaying;


	// Start is called before the first frame update
	private void Start()
	{
		Time.timeScale = 0;
		highScoreOffline.SetActive(false);
		highScoreOnline.SetActive(false);
		gameOverCanvas.SetActive(false);
		getReadyCanvas.SetActive(true);
		scoreCanvas.SetActive(false);

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

	public void GameOver(int actualScore)
	{
		gameOverCanvas.SetActive(true);
		Time.timeScale = 0;
		ch.PostScore(actualScore);
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

		int bestPersonalScore = ch.UserBestScores(highScore);

		//highScore.text = bestPersonalScore.ToString();

		scoreCanvas.SetActive(false);
		gameOverCanvas.SetActive(false);
		highScoreOnline.SetActive(false);
		highScoreOffline.SetActive(true);
	}

	public void ShowOnLineHighScore()
	{
		int newScore = scoreManager.score;
		actualScore.text = newScore.ToString();

		ch.BestHighScores(scoreTxt, scoreTxtParent);

		//highScore.text = bestPersonalScore.ToString();

		scoreCanvas.SetActive(false);
		gameOverCanvas.SetActive(false);
		highScoreOnline.SetActive(true);
		highScoreOffline.SetActive(false);
	}

	public void FromHSOfflineToGameOver()
	{
		scoreCanvas.SetActive(false);
		gameOverCanvas.SetActive(true);
		highScoreOnline.SetActive(false);
		highScoreOffline.SetActive(false);

	}
}