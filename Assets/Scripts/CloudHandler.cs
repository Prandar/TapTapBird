using CotcSdk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CloudHandler : MonoBehaviour
{
	public MainMenu mainMenu;

	private Cloud Cloud;
	private Gamer Gamer;
	// When a gamer is logged in, the loop is launched for domain private. Only one is run at once.
	private DomainEventLoop Loop;
	// Input field
	public TMP_InputField EmailInput;
	public TMP_InputField PwdInput;
	// Default parameters
	private const string DefaultEmailAddress = "admin@gmail.com";
	private const string DefaultPassword = "admin";

	private void Start()
	{
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
		Promise.UnhandledException += (object sender, ExceptionEventArgs e) => {
			Debug.LogError("Unhandled exception: " + e.Exception.ToString());
		};
		// Initiate getting the main Cloud object
		cb.GetCloud().Done(cloud => {
			Cloud = cloud;
			// Retry failed HTTP requests once
			Cloud.HttpRequestFailedHandler = (HttpRequestFailedEventArgs e) => {
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
		EmailInput.text = DefaultEmailAddress;
		PwdInput.text = DefaultPassword;

		VerificationGamerInPref();
	}

	private void Update()
	{

	}

	#region cotc
	// Signs in with an anonymous account
	public void DoLogin()
	{
		// Call the API method which returns an Promise<Gamer> (promising a Gamer result).
		// It may fail, in which case the .Then or .Done handlers are not called, so you
		// should provide a .Catch handler.
		Cloud.LoginAnonymously()
			.Then(gamer => DidLogin(gamer))
			.Catch(ex => {
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
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

	// Converts the account to e-mail
	public void DoConvertToEmail()
	{
		if (!RequireGamer()) return;
		Gamer.Account.Convert(
			network: LoginNetwork.Email.ToString().ToLower(),
			networkId: EmailInput.text,
			networkSecret: DefaultPassword)
		.Done(dummy => {
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
			networkId: EmailInput.text,
			networkSecret: PwdInput.text)
		.Done(this.DidLogin);
		
		SaveGamerDataInPref(EmailInput.text, PwdInput.text);
		VerificationGamerInPref();
	}
	
	public void DoLoginEmail(string id, string pwd)
	{
		Cloud.Login(
			network: "email",
			networkId: id,
			networkSecret: pwd)
		.Done(gamer =>
		{
			Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
			Debug.Log("Login data: " + gamer);
			Debug.Log("Server time: " + gamer["servertime"]);
			SaveGamerDataInPref(gamer.GamerId, gamer.GamerSecret);
		}, ex =>
		{
			// The exception should always be CotcException
			CotcException error = (CotcException)ex;
			Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
		});

		//VerificationGamerInPref();
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
	}

	private void ShowConnectionCanvas()
	{
		mainMenu.connectionCanvas.SetActive(true);
		mainMenu.mainMenuCanvas.SetActive(false);
		mainMenu.logIconCanvas.SetActive(false);
	}

	private void ShowMainMenuCanvas()
	{
		mainMenu.connectionCanvas.SetActive(false);
		mainMenu.mainMenuCanvas.SetActive(true);
	}

	private void ShowLoggedIcon()
	{
		mainMenu.logIconCanvas.SetActive(true);
	}
	#endregion CanvaDisplay

	#region PrefGamerData
	private void SaveGamerDataInPref(string id, string pwd)
	{
		PlayerPrefs.SetString("gamerID", id);
		PlayerPrefs.SetString("gamerPwd", pwd);
	}

	private string[] GetGamerDataInPref()
	{
		return new string[] { PlayerPrefs.GetString("gamerID"), PlayerPrefs.GetString("gamerPwd") };
	}

	private bool IsGamerDataExist()
	{
		return (PlayerPrefs.HasKey("gamerID") && PlayerPrefs.HasKey("gamerPwd"));
	}

	public void DebugGamerData()
	{
		if (IsGamerDataExist())
		{
			Debug.Log("pref Gamer Data __ id= " + GetGamerDataInPref()[0] + " pwd= " + GetGamerDataInPref()[1]);
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


	public void VerificationMailConnection()
	{
		if (EmailInput.text != "" || PwdInput.text !="" )
		{
			DoLoginEmail();
		} 
		else
		{
			//nothing
		}
	}

	#endregion Verification
}