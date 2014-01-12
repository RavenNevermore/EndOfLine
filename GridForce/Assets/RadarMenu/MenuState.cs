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
        Debug.Log("Starting game for " + this.arenaName);

        DontDestroyOnLoad(this);
        Application.LoadLevel(this.arenaName);


        NetworkConnectionError connectionError;
        MenuState menuState = GameObject.Find("state").GetComponent<MenuState>();

        if (menuState.type == MenuState.GameType.HOST)
        {
            Debug.Log("Starting new server");
            connectionError = Network.InitializeServer(32, menuState.portNumber, false);
        }
        else
        {
            Debug.Log("Connecting to server " + menuState.hostIp + " at port number " + menuState.portNumber.ToString());
            connectionError = Network.Connect(menuState.hostIp, menuState.portNumber);
        }

        if (connectionError != NetworkConnectionError.NoError)
        {
            this.OnFailedToConnect(connectionError);
        }
	}

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.LogError("-> Server error...");
    }


    void OnPlayerConnected(NetworkPlayer player)
    {
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
    }
}
