using UnityEngine;
using System.Collections;

public class MenuState : MonoBehaviour {

	public enum GameType {HOST, JOIN};

	public GameType type;
	public string hostName;
	public string hostIp;
    public int portNumber = 21496;

	public string arenaName;

	public string vehicleName;

	public string color;

	public void startGame()
    {
		NetworkConnectionError connectionError;

		if (this.type == MenuState.GameType.JOIN){
			Debug.Log("Connecting to server " + this.hostIp + " at port number " + this.portNumber.ToString());
			connectionError = Network.Connect(this.hostIp, this.portNumber);
			//this.networkView.RPC("getArenaName", RPCMode.Server, this.arenaName);
			if (connectionError != NetworkConnectionError.NoError){
				this.OnFailedToConnect(connectionError);
			}
		} else {

			this.initGameState();


	        
	        MenuState menuState = GameObject.Find("state").GetComponent<MenuState>();

	        if (menuState.type == MenuState.GameType.HOST)
	        {
	            Debug.Log("Starting new server");
	            connectionError = Network.InitializeServer(32, menuState.portNumber, false);

				if (connectionError != NetworkConnectionError.NoError){
					this.OnFailedToConnect(connectionError);
				}
	        }
	        /*else
	        {
	            Debug.Log("Connecting to server " + menuState.hostIp + " at port number " + menuState.portNumber.ToString());
	            connectionError = Network.Connect(menuState.hostIp, menuState.portNumber);
	        }*/

	        
		}
	}

	void initGameState(){
		Debug.Log("Starting game for " + this.arenaName);
		
		DontDestroyOnLoad(this);
		Application.LoadLevel(this.arenaName);
	}

	[RPC]
	void SetServerDetails(string servername, string arenaname){
		if (null == servername || "".Equals(servername.Trim())
		    && null == arenaname || "".Equals(arenaname.Trim()))
			return;

		this.arenaName = arenaname;
		this.hostName = servername;
		Debug.Log("Server details set from Network: " + this.hostName + " - " + this.arenaName);
		this.initGameState();
	}

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.LogError("-> Server error...");
    }


    void OnPlayerConnected(NetworkPlayer player)
    {
		Debug.Log("Sending game details to player: " + player);
        if (this.networkView != null)
		    this.networkView.RPC("SetServerDetails", player, this.hostName, this.arenaName);
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
    }
}
