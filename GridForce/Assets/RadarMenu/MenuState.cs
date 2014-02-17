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

    private int numConnections = 0;

    public ErrorState errorState = null;

    public bool gameStarted = false;

    private Dictionary<NetworkPlayer, bool> playersReady = new Dictionary<NetworkPlayer, bool>();
    public Dictionary<NetworkPlayer, string> playerNames = new Dictionary<NetworkPlayer, string>();
    public string[] playerNameListOld = new string[4] { "", "", "", "" };
    public string[] playerNameList = new string[4] { "", "", "", "" };

    void Start()
    {
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            this.errorState = errorStateObject.GetComponent<ErrorState>();

        GameObject otherMenuState = GameObject.Find("MenuState");
        if (otherMenuState != this.gameObject && otherMenuState != null)
        {
            UnityEngine.Object.Destroy(this.gameObject.GetComponent<NetworkView>());
            UnityEngine.Object.Destroy(this);
            if (this.name == "MenuState")
                UnityEngine.Object.Destroy(this.gameObject);
        }

#if UNITY_STANDALONE || UNITY_EDITOR

        this.playerName = System.Environment.MachineName;

#endif
    }

	public void StartGame()
    {
        this.playersReady.Clear();
        this.playerNames.Clear();
        this.playerNameListOld = new string[4] { "", "", "", "" };
        this.playerNameList = new string[4] { "", "", "", "" };

		NetworkConnectionError connectionError;
        Network.maxConnections = 3;

        this.gameStarted = false;

		if (this.type == MenuState.GameType.HOST)
        {
            this.InitGameState();

            Debug.Log("Starting new server");
            connectionError = Network.InitializeServer(Network.maxConnections, this.portNumber, false);

            if (connectionError != NetworkConnectionError.NoError)
            {
                Debug.LogWarning("Server initialisation failed");

                Network.Disconnect(200);
                this.gameStarted = false;

                this.errorState.ClearButtons();
                this.errorState.AddLine("Server initialisation failed", true);
                this.errorState.AddButton("Main Menu", this.ReturnToMainMenu);
                this.errorState.AddButton("Retry", this.HostGameRetry);
                this.errorState.Show();
            }
            else
            {
                this.errorState.ClearButtons();
                this.errorState.AddLine("Server started", false);
                this.errorState.AddLine("Waiting for players...", false);
                this.errorState.Show(3.0f);
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
            this.networkView.RPC("ClientReadyRPC", RPCMode.Server, Network.player, this.playerName);
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
    void ClientReadyRPC(NetworkPlayer player, string name)
    {
        if (this.playersReady.ContainsKey(player))
            this.playersReady[player] = true;
        else
            this.playersReady.Add(player, true);

        this.errorState.ClearButtons();
        this.errorState.AddLine("Player " + name + " is ready", false);
        this.errorState.Show(3.0f);
    }


    void OnPlayerConnected(NetworkPlayer player)
    {
        if (!(this.playersReady.ContainsKey(player)))
            this.playersReady.Add(player, false);

        this.numConnections++;
    }


    [RPC]
    void SetClientName(NetworkPlayer player, string name)
    {
        this.errorState.ClearButtons();
        this.errorState.AddLine("Player " + name + " connected", false);
        this.errorState.Show(3.0f);

        Debug.Log("Sending game details to player: " + player);
        if (this.networkView != null)
        {
            this.networkView.RPC("SetServerDetails", player, this.hostName, this.arenaName);
            this.networkView.RPC("PlayerConnectedNotification", RPCMode.Others, player, name);
        }

        if (!(this.playerNames.ContainsKey(player)))
            this.playerNames.Add(player, name);
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
        if (this.playersReady.ContainsKey(player))
            this.playersReady.Remove(player);
        string name = "";
        if (this.playerNames.ContainsKey(player))
        {
            name = this.playerNames[player];
            this.playerNames.Remove(player);
        }

        this.numConnections--;

        if (this.gameStarted)
            Network.maxConnections = Network.connections.Length;

        if ((this.numConnections <= 0 || this.playersReady.Count <= 0) && this.gameStarted)
        {
            Debug.LogWarning("All players disconnected");

            this.gameStarted = false;
            Network.Disconnect(200);

            this.errorState.Clear();
            this.errorState.AddLine("Player " + name + " disconnected", false);
            this.errorState.AddLine("All players disconnected", true);
            this.errorState.AddButton("Main Menu", this.ReturnToMainMenu);
            this.errorState.AddButton("Restart", this.HostGameRetry);
            this.errorState.Show();
        }
        else
        {
            if (this.networkView != null)
                this.networkView.RPC("PlayerDisconnectedNotification", RPCMode.All, player, name);
            else
                this.PlayerDisconnectedNotification(player, name);
        }
    }

    public void RequestNameUpdate(NetworkPlayer player)
    {
        this.networkView.RPC("RequestNameUpdateRPC", RPCMode.Server, Network.player);
    }

    public void UpdateNames(string name1, string name2, string name3, string name4)
    {
        this.networkView.RPC("UpdateNamesRPC", RPCMode.Others, this.playerNameList[0], this.playerNameList[1], this.playerNameList[2], this.playerNameList[3]);
    }

    [RPC]
    void RequestNameUpdateRPC(NetworkPlayer player)
    {
        this.networkView.RPC("UpdateNamesRPC", player, this.playerNameList[0], this.playerNameList[1], this.playerNameList[2], this.playerNameList[3]);
    }

    [RPC]
    void UpdateNamesRPC(string name1, string name2, string name3, string name4)
    {
        this.playerNameList[0] = name1;
        this.playerNameList[1] = name2;
        this.playerNameList[2] = name3;
        this.playerNameList[3] = name4;

        this.playerNameListOld[0] = this.playerNameList[0];
        this.playerNameListOld[1] = this.playerNameList[1];
        this.playerNameListOld[2] = this.playerNameList[2];
        this.playerNameListOld[3] = this.playerNameList[3];
    }

    [RPC]
    void PlayerDisconnectedNotification(NetworkPlayer player, string name)
    {
        this.errorState.ClearButtons();
        this.errorState.AddLine("Player " + name + " disconnected", false);
        this.errorState.Show(3.0f);
    }

    [RPC]
    void PlayerConnectedNotification(NetworkPlayer player, string name)
    {
        this.errorState.ClearButtons();
        this.errorState.AddLine("Player " + name + " connected", false);
        this.errorState.Show(3.0f);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (!(Network.isServer))
        {
            Debug.LogWarning("Disconnected from server");

            this.gameStarted = false;

            this.errorState.Clear();
            this.errorState.AddLine("Disconnected from server", true);
            this.errorState.AddButton("Main Menu", this.ReturnToMainMenu);
            this.errorState.Show();

            this.playerNameListOld = new string[4] { this.playerName, "", "", "" };
            this.playerNameList = new string[4] { this.playerName, "", "", "" };
        }
    }

    void HostGameRetry()
    {
        this.errorState.AddLine("Restarting server...", false);
        this.StartGame();
    }

    void ReturnToMainMenu()
    {
        Application.LoadLevel("MainMenu");
    }
}
