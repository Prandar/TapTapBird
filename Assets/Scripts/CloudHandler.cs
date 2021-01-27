using CotcSdk;
using System;
using UnityEngine;

public class CloudHandler : MonoBehaviour
{
	private MainMenu mainMenu;

	private Cloud Cloud;
	private Gamer Gamer;
	private string[] gamerInPref;

	// When a gamer is logged in, the loop is launched for domain private. Only one is run at once.
	private DomainEventLoop Loop;

	// Default parameters
	private const string DefaultEmailAddress = "admin@gmail.com";

	private const string DefaultPassword = "admin";

	private void Awake()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag("CloudHandler");

		if (objs.Length > 1)
		{
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
	}

	public void Init()
	{
		GameObject obj = GameObject.FindGameObjectWithTag("MainMenu");
		mainMenu = obj.GetComponent<MainMenu>();

		DisableAllCanvas();
		//DebugClearPlayerPref(true);
		DebugGamerData();


		// Link with the CotC Game Object
		var cb = FindObjectOfType<CotcGameObject>();
		if (cb == null)
		{
			Debug.LogError("Please put a Clan of the Cloud prefab in your scene!");
			return;
		}
		// Log unhandled exceptions (.Done block without .Catch -- not called if there is any .Then)
		Promise.UnhandledException += (object sender, ExceptionEventArgs e) =>
		{
			Debug.LogError("Unhandled exception: " + e.Exception.ToString());
		};
		// Initiate getting the main Cloud object
		cb.GetCloud().Done(cloud =>
		{
			Cloud = cloud;
			// Retry failed HTTP requests once
			Cloud.HttpRequestFailedHandler = (HttpRequestFailedEventArgs e) =>
			{
				if (e.UserData == null)
				{
					e.UserData = new object();
					e.RetryIn(1000);
				}
				else
					e.Abort();
			};
			Debug.Log("Setup done");
		});
		// Use a default text in the e-mail address
		mainMenu.EmailInput.text = DefaultEmailAddress;
		mainMenu.PwdInput.text = DefaultPassword;

		VerificationGamerInPref();
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
	}

	#region cotc

	// Signs in with an anonymous account
	private void DoLogin()
	{
		var cotc = FindObjectOfType<CotcGameObject>();

		// Call the API method which returns an Promise<Gamer> (promising a Gamer result).
		cotc.GetCloud().Done(cloud =>
		{
			cloud.LoginAnonymously()
			.Done(gamer =>
			{
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);
				SaveGamerDataInPref(gamer.GamerId, gamer.GamerSecret, gamer.Network);
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	public void DoLoginOnClick()
	{
		DoLogin();
		ShowMainMenuCanvas();
		ShowLoggedIcon();
	}

	// Converts the account to e-mail
	public void DoConvertToEmail()
	{
		if (!RequireGamer()) return;
		Gamer.Account.Convert(
			network: LoginNetwork.Email.ToString().ToLower(),
			networkId: mainMenu.EmailInput.text,
			networkSecret: DefaultPassword)
		.Done(dummy =>
		{
			Debug.Log("Successfully converted account");
		});
	}

	// Log in by e-mail
	public void DoLoginEmail()
	{
		// You may also not provide a .Catch handler and use .Done instead of .Then. In that
		// case the Promise.UnhandledException handler will be called instead of the .Done
		// block if the call fails.
		Cloud.Login(
			network: LoginNetwork.Email.Describe(),
			networkId: mainMenu.EmailInput.text,
			networkSecret: mainMenu.PwdInput.text)
		.Done(this.DidLogin);

		SaveGamerDataInPref(mainMenu.EmailInput.text, mainMenu.PwdInput.text, "email");
		VerificationGamerInPref();
	}

	//public void DoLoginEmail(string id, string pwd)
	//{
	//	Cloud.Login(
	//		network: "email",
	//		networkId: id,
	//		networkSecret: pwd)
	//	.Done(gamer =>
	//	{
	//		Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
	//		Debug.Log("Login data: " + gamer);
	//		Debug.Log("Server time: " + gamer["servertime"]);
	//		SaveGamerDataInPref(gamer.GamerId, gamer.GamerSecret, gamer.Network);
	//	}, ex =>
	//	{
	//		// The exception should always be CotcException
	//		CotcException error = (CotcException)ex;
	//		Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
	//	});

		//VerificationGamerInPref();
	//}

	public void LogOutOnClick()
	{
		var cotc = FindObjectOfType<CotcGameObject>();
		gamerInPref = GetGamerDataInPref();
		//DebugGamerData();

		cotc.GetCloud().Done(cloud =>
		{
			Cloud.Login(
				network: gamerInPref[2],
				networkId: gamerInPref[0],
				networkSecret: gamerInPref[1])
			.Done(gamer =>
			{
				Debug.Log("reLogIn to LogOut");
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);

				cloud.Logout(gamer)
				.Done(result =>
				{
					Debug.Log("Logout succeeded");
					DebugClearPlayerPref(false);
				}, ex =>
				{
					// The exception should always be CotcException
					CotcException error = (CotcException)ex;
					Debug.LogError("Failed to logout: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
				});
			}, ex =>
			{
				//DebugGamerData();
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	private void ResumeSession()
	{
		var cotc = FindObjectOfType<CotcGameObject>();
		gamerInPref = GetGamerDataInPref();

		cotc.GetCloud().Done(cloud =>
		{
			Cloud.ResumeSession(
				gamerId: gamerInPref[0],
				gamerSecret: gamerInPref[1])
			.Done(gamer =>
			{
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);
				Gamer = gamer;
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	public void InvokeCreateKvStoreKeyUserBatch()
	{
		string adminID = DefaultEmailAddress;
		string gamerID2 = GetGamerDataInPref()[0];

		Bundle kvStoreAcl = Bundle.CreateObject();
		kvStoreAcl["r"] = new Bundle("*");
		kvStoreAcl["w"] = Bundle.CreateArray(new Bundle[] { new Bundle(adminID), new Bundle(gamerID2) });
		kvStoreAcl["a"] = Bundle.CreateArray(new Bundle[] { new Bundle(adminID) });

		// Or: Bundle kvStoreAcl = Bundle.FromJson("{\"r\":\"*\",\"w\":[\"gamerID1\",\"gamerID2\"],\"a\":[\"gamerID1\"]}");

		Bundle batchParams = Bundle.CreateObject();
		batchParams["keyName"] = new Bundle("HighScore");
		batchParams["keyValue"] = new Bundle("HighScoreValue");
		batchParams["keyAcl"] = kvStoreAcl;

		// currentGamer is an object retrieved after one of the different Login functions.
		var cotc = FindObjectOfType<CotcGameObject>();
		gamerInPref = GetGamerDataInPref();
		//DebugGamerData();

		cotc.GetCloud().Done(cloud =>
		{
			Cloud.Login(
				network: gamerInPref[2],
				networkId: gamerInPref[0],
				networkSecret: gamerInPref[1])
			.Done(gamer =>
			{
				Debug.Log("reLogIn to KV");
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);

				gamer.Batches.Domain("private").Run("KvStore_CreateKey", batchParams).Done(
					// You may want to check for success with: if (result["n"].AsInt() == 1)
					delegate (Bundle result) { Debug.Log("Success Create Key: " + result.ToString()); },
					delegate (Exception error) { Debug.LogError("Error Create Key: " + error.ToString()); }
				);
			});
		});
	}

	public void PostScore(int actualScore)
	{
		var cotc = FindObjectOfType<CotcGameObject>();
		gamerInPref = GetGamerDataInPref();
		//DebugGamerData();

		cotc.GetCloud().Done(cloud =>
		{
			Cloud.Login(
				network: gamerInPref[2],
				networkId: gamerInPref[0],
				networkSecret: gamerInPref[1])
			.Done(gamer =>
			{
				Debug.Log("reLogIn to postOnLeaderBoard");
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);

				gamer.Scores.Domain("private").Post(actualScore, "World", ScoreOrder.HighToLow,
				"nocheat", false)
				.Done(postScoreRes =>
				{
					Debug.Log("Post score: " + postScoreRes.ToString());
				}, ex =>
				{
					// The exception should always be CotcException
					CotcException error = (CotcException)ex;
					Debug.LogError("Could not post score: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
				});
			});
		});
	}

	public int UserBestScores()
	{
		int value = -1;
		var cotc = FindObjectOfType<CotcGameObject>();
		gamerInPref = GetGamerDataInPref();
		DebugGamerData();

		cotc.GetCloud().Done(cloud =>
		{
			Cloud.Login(
				network: gamerInPref[2],
				networkId: gamerInPref[0],
				networkSecret: gamerInPref[1])
			.Done(gamer =>
			{
				Debug.Log("reLogIn to getPersonalLeaderBoard");
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);

				gamer.Scores.Domain("private").ListUserBestScores()
				.Done(listUserBestScoresRes =>
				{
					foreach (var score in listUserBestScoresRes)
					{
						if (score.Key == "World")
						{
							value = Convert.ToInt32(score.Value.Value);
							Debug.Log("Coucou " +value);
							PlayerPrefs.SetInt("HighScore", value);

						}
						Debug.Log(score.Key + ": " + score.Value.Value);

					}
				}, ex =>
				{
					// The exception should always be CotcException
					CotcException error = (CotcException)ex;
					Debug.LogError("Could not get user best scores: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
				});
			});
		});

		return PlayerPrefs.GetInt("HighScore");
	}


	// Invoked when any sign in operation has completed
	private void DidLogin(Gamer newGamer)
	{
		if (Gamer != null)
		{
			Debug.LogWarning("Current gamer " + Gamer.GamerId + " has been dismissed");
			Loop.Stop();
		}
		Gamer = newGamer;
		Loop = Gamer.StartEventLoop();
		Loop.ReceivedEvent += Loop_ReceivedEvent;
		Debug.Log("Signed in successfully (ID = " + Gamer.GamerId + ")");
	}

	private void Loop_ReceivedEvent(DomainEventLoop sender, EventLoopArgs e)
	{
		Debug.Log("Received event of type " + e.Message.Type + ": " + e.Message.ToJson());
	}

	private bool RequireGamer()
	{
		if (Gamer == null)
			Debug.LogError("You need to login first. Click on a login button.");
		return Gamer != null;
	}

	#endregion cotc

	#region CanvaDisplay

	private void DisableAllCanvas()
	{
		mainMenu.connectionCanvas.SetActive(false);
		mainMenu.mainMenuCanvas.SetActive(false);
		mainMenu.logIconCanvas.SetActive(false);
		mainMenu.logOutCanvas.SetActive(false);
	}

	private void ShowConnectionCanvas()
	{
		mainMenu.connectionCanvas.SetActive(true);
		mainMenu.mainMenuCanvas.SetActive(false);
		mainMenu.logIconCanvas.SetActive(false);
		mainMenu.logOutCanvas.SetActive(false);
	}

	private void ShowMainMenuCanvas()
	{
		mainMenu.connectionCanvas.SetActive(false);
		mainMenu.mainMenuCanvas.SetActive(true);
		mainMenu.logOutCanvas.SetActive(false);
	}

	private void ShowLoggedIcon()
	{
		mainMenu.logIconCanvas.SetActive(true);
		mainMenu.logOutCanvas.SetActive(false);
	}

	#endregion CanvaDisplay

	#region PrefGamerData

	private void SaveGamerDataInPref(string id, string pwd, string network)
	{
		PlayerPrefs.SetString("gamerID", id);
		PlayerPrefs.SetString("gamerPwd", pwd);
		PlayerPrefs.SetString("gamerNetwork", network);
	}

	private string[] GetGamerDataInPref()
	{
		return new string[] {
			PlayerPrefs.GetString("gamerID"),
			PlayerPrefs.GetString("gamerPwd"),
			PlayerPrefs.GetString("gamerNetwork")
		};
	}

	private bool IsGamerDataExist()
	{
		return (PlayerPrefs.HasKey("gamerID") && PlayerPrefs.HasKey("gamerPwd") && PlayerPrefs.HasKey("gamerNetwork"));
	}

	public void DebugGamerData()
	{
		if (IsGamerDataExist())
		{
			Debug.Log("pref Gamer Data __ id= " + GetGamerDataInPref()[0] + " || pwd= " + GetGamerDataInPref()[1] + " || network= " + GetGamerDataInPref()[2]);
		}
		else
		{
			Debug.Log("pref Gamer Data __ EMPTY");
		}
	}

	public void DebugClearPlayerPref(bool deleteAll)
	{
		if (deleteAll)
		{
			PlayerPrefs.DeleteAll();
		}
		else
		{
			PlayerPrefs.DeleteKey("gamerId");
			PlayerPrefs.DeleteKey("gamerPwd");
			PlayerPrefs.DeleteKey("gamerNetwork");
		}
	}

	#endregion PrefGamerData

	#region Verification

	public bool VerificationGamerInPref()
	{
		if (IsGamerDataExist())
		{
			//DoLoginEmail(GetGamerDataInPref()[0], GetGamerDataInPref()[1]);
			ShowMainMenuCanvas();
			ShowLoggedIcon();
		}
		else
		{
			ShowConnectionCanvas();
		}
		return IsGamerDataExist();
	}

	//public void VerificationMailConnection()
	//{
	//	if (mainMenu.EmailInput.text != "" || mainMenu.PwdInput.text != "")
	//	{
	//		DoLoginEmail();
	//	}
	//	else
	//	{
	//
	//	}
	//}

	#endregion Verification
}