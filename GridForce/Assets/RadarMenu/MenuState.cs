using UnityEngine;
using System;
using System.Collections;

public class MenuState : MonoBehaviour
{
	public enum GameType {HOST, JOIN};

	public GameType type;
	public string hostName;
	public string hostIp;
    public int portNumber = 21496;

	public string arenaName;

    public int vehicleSelection = 0;

    public string playerName = "PLAYER NAME";

    public GameObject playerDisconnectedNotification = null;

    private int numConnections = 0;
    public bool gameStarted = false;

	public void startGame()
    {
		NetworkConnectionError connectionError;
        Network.maxConnections = 3;

		if (this.type == MenuState.GameType.JOIN)
        {
			Debug.Log("Connecting to server " + this.hostIp + " at port number " + this.portNumber.ToString());
			connectionError = Network.Connect(this.hostIp, this.portNumber);
		}
        else
        {
			this.initGameState();	        

	        if (this.type == MenuState.GameType.HOST)
	        {
	            Debug.Log("Starting new server");
                connectionError = Network.InitializeServer(32, this.portNumber, false);

                if (connectionError != NetworkConnectionError.NoError)
                {
                    Debug.LogWarning("Server initialisation failed");
                    Application.LoadLevel("ServerInitFailed");
                }
	        }      
		}
	}

	void initGameState()
    {
		Debug.Log("Starting game for " + this.arenaName);
		
		DontDestroyOnLoad(this);
		Application.LoadLevel(this.arenaName);
	}

	[RPC]
	void SetServerDetails(string servername, string arenaname)
    {
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
        Debug.LogWarning("Connection to server failed");
        Application.LoadLevel("ConnectionFailed");
    }


    void OnPlayerConnected(NetworkPlayer player)
    {
        this.numConnections++;

		Debug.Log("Sending game details to player: " + player);
        if (this.networkView != null)
		    this.networkView.RPC("SetServerDetails", player, this.hostName, this.arenaName);
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
        this.numConnections--;

        if (this.numConnections <= 0 && this.gameStarted)
        {
            Debug.LogWarning("All players dissconnected");
            Application.LoadLevel("AllConnectionsLost");
            this.gameStarted = false;
        }

        try
        {
            Debug.LogWarning("Player " + player.ipAddress + " dissconnected from the server");
            UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.playerDisconnectedNotification, Vector3.zero, Quaternion.identity);
            ((GameObject)(newObject)).GetComponentInChildren<GUIText>().text = "PLAYER " + player.ipAddress + " DISCONNECTED FROM THE SERVER...";
            DontDestroyOnLoad(newObject);
        }
        catch (Exception)
        {
        }
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (!(Network.isServer))
        {
            Debug.LogWarning("Disconnected from server");
            Application.LoadLevel("ConnectionLost");
        }
    }
}
