using CotcSdk;
using TMPro;
using UnityEngine;

public class LogInController : MonoBehaviour
{
	public TMP_InputField login;
	public TMP_InputField password;

	public MainMenu mainMenu;

	private Gamer actualGamer;
	private string loginValue;
	private string passwordValue;
	private string loginKey = "id";
	private string passwordKey = "pwd";

	private void Start()
	{
		//LoginAnonymous();
		//SetGamerData(GetGamerDataFromPref()[0], GetGamerDataFromPref()[1]);
		DisableAllCanvas();

		DebugClearPlayerPref(true);

		verifGamerConnection();
	}

	private void Update()
	{
		//GetGamerDataFromPref();
		//verifGamerConnection();
	}

	#region ABCD

	private void DisableAllCanvas()
	{
		mainMenu.connectionCanvas.SetActive(false);
		mainMenu.mainMenuCanvas.SetActive(false);
	}

	private void verifGamerConnection()
	{
		string[] gamerDatafromPref = GetGamerDataFromPref();
		Debug.Log("ROM1 id=" + gamerDatafromPref[0]);
		Debug.Log("ROM1 pwd=" + gamerDatafromPref[1]);

		if (gamerDatafromPref[0] != "" && gamerDatafromPref[1] != "")
		{
			Debug.Log("ROM1 if");
			SetGamerData(gamerDatafromPref[0], gamerDatafromPref[1]);
			//ResumeSession();

			if (false)
			{
				//show icon anon
			}
			else if (false)
			{
				//show icon logged
			}
			else
			{
				Debug.Log("ROM1 else");
				mainMenu.connectionCanvas.SetActive(false);
				mainMenu.mainMenuCanvas.SetActive(true);
			}
		}
		else
		{
			Debug.Log("ROM1 else else");
			mainMenu.connectionCanvas.SetActive(true);
			//NewAnonymousGamer();
		}
	}

	public void NewAnonymousGamer()
	{
		LoginAnonymous();
		Debug.Log("ROM1 0001");
		verifGamerConnection();
		Debug.Log("ROM1 0002");
	}

	#endregion ABCD

	public void Logout(Gamer gamer)
	{
		//var gamer; // gamer was retrieved previously with a call to one of the Login methods from `Clan`
		var cotc = FindObjectOfType<CotcGameObject>();

		cotc.GetCloud().Done(cloud =>
		{
			cloud.Logout(gamer)
			.Done(result =>
			{
				Debug.Log("Logout succeeded");
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to logout: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	public void LoginAnonymous()
	{
		var cotc = FindObjectOfType<CotcGameObject>();

		cotc.GetCloud().Done(cloud =>
		{
			cloud.LoginAnonymously()
			.Done(gamer =>
			{
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);
				SetGamerData(gamer.GamerId, gamer.GamerSecret);
				SetGamerDataInPref(gamer.GamerId, gamer.GamerSecret);
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	public void LoginNetwork()
	{
		var cotc = FindObjectOfType<CotcGameObject>();

		cotc.GetCloud().Done(cloud =>
		{
			cloud.Login(
				network: "email",
				networkId: loginValue,
				networkSecret: passwordValue)
			.Done(gamer =>
			{
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	//void Convert(Gamer currentGamer)
	//{
	//    // currentGamer is an object retrieved after one of the different Login functions.

	//    currentGamer.Account.Convert(LoginNetwork.Email, "myEmail@gmail.com", "myPassword")
	//    .Done(convertRes => {
	//        Debug.Log("Convert succeeded: " + convertRes.ToString());
	//    }, ex => {
	//        // The exception should always be CotcException
	//        CotcException error = (CotcException)ex;
	//        Debug.LogError("Failed to convert: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
	//    });
	//}

	private void ResumeSession()
	{
		var cotc = FindObjectOfType<CotcGameObject>();

		cotc.GetCloud().Done(cloud =>
		{
			cloud.ResumeSession(
				gamerId: loginValue,
				gamerSecret: passwordValue)
			.Done(gamer =>
			{
				Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
				Debug.Log("Login data: " + gamer);
				Debug.Log("Server time: " + gamer["servertime"]);
				SaveGamer(gamer);
			}, ex =>
			{
				// The exception should always be CotcException
				CotcException error = (CotcException)ex;
				Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
			});
		});
	}

	public void SetGamerData(string gamerId, string gamerSecret)
	{
		loginValue = gamerId;
		passwordValue = gamerSecret;
		Debug.Log("set " + gamerId + " " + gamerSecret);
	}

	public void SetGamerDataInPref(string gamerId, string gamerSecret)
	{
		PlayerPrefs.SetString(loginKey, gamerId);
		PlayerPrefs.SetString(passwordKey, gamerSecret);
	}

	public string[] GetGamerDataFromPref()
	{
		if (PlayerPrefs.GetString(PlayerPrefs.GetString(loginKey)) != null && PlayerPrefs.GetString(PlayerPrefs.GetString(passwordKey)) != null)
		{
			string[] value = new string[] { PlayerPrefs.GetString(loginKey), PlayerPrefs.GetString(passwordKey) };
			Debug.Log("--get-- " + value[0] + " " + value[1]);
			return value;
		}
		else
		{
			Debug.Log("--get-- no data stored");
			return null;
		}
	}

	public void SaveGamer(Gamer gamer)
	{
		actualGamer = gamer;
	}

	public void DebugClearPlayerPref(bool deleteAll)
	{
		if (deleteAll)
		{
			PlayerPrefs.DeleteAll();
		}
		else
		{
			PlayerPrefs.DeleteKey(loginKey);
			PlayerPrefs.DeleteKey(passwordKey);
		}
	}
}