using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

    public GameObject serverNotification = null;

    private int numConnections = 0;
    public bool gameStarted = false;

    private Dictionary<NetworkPlayer, bool> playersReady = new Dictionary<NetworkPlayer, bool>();

    void Start()
    {
        if (GameObject.Find("MenuState") != this.gameObject)
        {
            UnityEngine.Object.Destroy(this.gameObject.GetComponent<NetworkView>());
            UnityEngine.Object.Destroy(this);
        }
    }

	public void StartGame()
    {
		NetworkConnectionError connectionError;
        Network.maxConnections = 3;

		if (this.type == MenuState.GameType.HOST)
        {
            this.InitGameState();

            Debug.Log("Starting new server");
            connectionError = Network.InitializeServer(32, this.portNumber, false);

            if (connectionError != NetworkConnectionError.NoError)
            {
                Debug.LogWarning("Server initialisation failed");
                Application.LoadLevel("ServerInitFailed");
            }
		}
        else
            this.ClientReady();
	}


    public void ConnectAsClient()
    {
        Debug.Log("Connecting to server " + this.hostIp + " at port number " + this.portNumber.ToString());
        Network.Connect(this.hostIp, this.portNumber);
    }


	void InitGameState()
    {
		Debug.Log("Starting game for " + this.arenaName);
		
		DontDestroyOnLoad(this);
		Application.LoadLevel(this.arenaName);
	}

    public void ClientReady()
    {
        this.InitGameState();

        if (this.networkView != null)
            this.networkView.RPC("ClientReadyRPC", RPCMode.Server, Network.player);
    }


    public bool AllPlayersReady()
    {
        foreach (KeyValuePair<NetworkPlayer, bool> pair in this.playersReady)
            if (!(pair.Value)) return false;

        return true;
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
	}

    [RPC]
    void ClientReadyRPC(NetworkPlayer player)
    {
        if (this.playersReady.ContainsKey(player))
            this.playersReady[player] = true;
        else
            this.playersReady.Add(player, true);
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.LogWarning("Connection to server failed");
        Application.LoadLevel("ConnectionFailed");
    }


    void OnPlayerConnected(NetworkPlayer player)
    {
        if (!(this.playersReady.ContainsKey(player)))
            this.playersReady.Add(player, false);

        this.numConnections++;

        try
        {
            Debug.LogWarning("Player " + player.ipAddress + " connected to the server");
            UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.serverNotification, Vector3.zero, Quaternion.identity);
            ((GameObject)(newObject)).GetComponentInChildren<GUIText>().text = "PLAYER " + player.ipAddress + " CONNECTED TO THE SERVER...";
            DontDestroyOnLoad(newObject);
        }
        catch (Exception)
        {
        }

		Debug.Log("Sending game details to player: " + player);
        if (this.networkView != null)
		    this.networkView.RPC("SetServerDetails", player, this.hostName, this.arenaName);
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
        if (this.playersReady.ContainsKey(player))
            this.playersReady.Remove(player);

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
            UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.serverNotification, Vector3.zero, Quaternion.identity);
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
