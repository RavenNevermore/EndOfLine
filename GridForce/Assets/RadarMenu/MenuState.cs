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

    public GameObject gameStatePrefab = null;
    public GameObject selectControlsPrefab = null;
    public bool useButtonControls = false;
    public bool selectingControls = false;
    public bool loadedLevel = false;

    private GameObject controlSelectionInstance = null;

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

        //#if UNITY_STANDALONE || UNITY_EDITOR

        this.playerName = System.Environment.MachineName;

        //#endif
    }

    void Update()
    {
        this.WaitForPlayers();
    }

	public void StartGame()
    {
        this.controlSelectionInstance = null;
        this.playersReady.Clear();
        this.playerNames.Clear();
        this.playerNameListOld = new string[4] { "", "", "", "" };
        this.playerNameList = new string[4] { "", "", "", "" };
        this.useButtonControls = false;

		NetworkConnectionError connectionError;
        Network.maxConnections = 3;

        this.gameStarted = false;

		if (this.type == MenuState.GameType.HOST)
        {
            this.InitGameState();

            Debug.Log("Starting new server");
            connectionError = Network.InitializeServer(Network.maxConnections, this.portNumber, false);

            if (!(this.playersReady.ContainsKey(Network.player)))
                this.playersReady.Add(Network.player, false);

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
            this.InitGameState();
	}


    public void ProceedToControlSelection()
    {
        Network.maxConnections = Network.connections.Length;

        if (MenuState.GameType.HOST == this.type)
        {
            try
            {
                UdpBroadcasting.destroyBeacon();
                Debug.Log("Beacon has been taken down!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        if (Network.connections.Length > 0 && this.networkView != null)
            this.networkView.RPC("ShowControlSelectionScreenRPC", RPCMode.All);
        else
            this.ShowControlSelectionScreenRPC();
    }

    [RPC]
    void ShowControlSelectionScreenRPC()
    {
        this.selectingControls = true;

        if (!(this.loadedLevel))
            return;

        StartHostedGameBehaviour[] startButtonBehaviors = GameObject.FindObjectsOfType<StartHostedGameBehaviour>();
        for (int i = 0; i < startButtonBehaviors.GetLength(0); i++)
            startButtonBehaviors[i].HideAll();

        this.controlSelectionInstance = (GameObject)(UnityEngine.Object.Instantiate(this.selectControlsPrefab, this.selectControlsPrefab.transform.position, this.selectControlsPrefab.transform.rotation));
        
        SelectControlBehavior[] buttonScripts = this.controlSelectionInstance.GetComponentsInChildren<SelectControlBehavior>();
        for (int i = 0; i < buttonScripts.GetLength(0); i++)
        {
            buttonScripts[i].menuState = this;
        }

        this.loadedLevel = false;
    }

    public void ControlsSelected()
    {
        this.ClientReady();
    }

    void WaitForPlayers()
    {
        if (!(this.selectingControls) || (Network.connections.Length > 0 && !(this.networkView.isMine)))
            return;

        Network.maxConnections = Network.connections.Length;

        if (this.AllPlayersReady())
        {
            this.InstantiateGameState();
            if (Network.connections.Length > 0 && this.networkView != null)
                this.networkView.RPC("DoneSelectingControlsRPC", RPCMode.All);
            else
                this.DoneSelectingControlsRPC();
        }
    }

    [RPC]
    void DoneSelectingControlsRPC()
    {
        this.selectingControls = false;

        UnityEngine.Object.Destroy(this.controlSelectionInstance);
    }

    void InstantiateGameState()
    {
        GameObject gameState = null;
        if (Network.connections.Length > 0)
            gameState = (GameObject)UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
        else
            gameState = (GameObject)UnityEngine.Object.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity);

        this.ReinstatiateObjectsIfNeccesary();

        gameState.GetComponent<GameState>().menuState = this;
        this.gameStarted = true;
    }

    void ReinstatiateObjectsIfNeccesary()
    {
        GameObject[] itemBoxes = GameObject.FindGameObjectsWithTag("ItemBox");
        ItemBoxBehavior itemBoxScript = null;
        foreach (GameObject itemBox in itemBoxes)
        {
            itemBoxScript = itemBox.GetComponent<ItemBoxBehavior>();
            if (itemBoxScript != null)
                itemBoxScript.ReInstantiate();
        }
        if (itemBoxScript != null)
            itemBoxScript.KillAllLocal();

        GameObject[] pillarObjects = GameObject.FindGameObjectsWithTag("Pillar");
        PillarBehavior pillarScript = null;
        foreach (GameObject pillar in pillarObjects)
        {
            pillarScript = pillar.GetComponent<PillarBehavior>();
            if (pillarScript != null)
                pillarScript.ReInstantiate();
        }
        if (pillarScript != null)
            pillarScript.KillAllLocal();
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

        this.loadedLevel = true;
	}

    void OnLevelWasLoaded(int level)
    {
        if (this.loadedLevel && this.selectingControls)
        {
            this.ShowControlSelectionScreenRPC();
        }
    }

    public void ClientReady()
    {
        if (Network.connections.Length > 0 && this.networkView != null)
            this.networkView.RPC("ClientReadyRPC", RPCMode.All, Network.player, this.playerName);
        else
            this.ClientReadyRPC(Network.player, this.playerName);
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
