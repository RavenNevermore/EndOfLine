using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using MultiPlatform;


public enum MainMenuSelection
{
    MainMenu,
    HostGame,
    WaitForPlayers,
    SetupGame,
    JoinGame
}

public enum SelectedArena
{
    Arena1,
    Arena2
}

public class MainMenu : MonoBehaviour
{
    public GUISkin guiSkin = null;      // The GUI skin to use for the main menu
    public MainMenuSelection menuSelection = MainMenuSelection.MainMenu;    // Current menu selection
    public string enteredIP = "";    // The currently entered IP
    public SelectedArena selectedArena = SelectedArena.Arena1;     // The selected arena
    public string enteredName = "";    // Host name

    private NetworkComponents.TCPConnector connector = null;    // TCP connector
	private NetworkComponents.UDPBroadcaster broadcaster = null;	// UDP broadcaster
	private NetworkComponents.UDPReceiver receiver = null;		// UDP receiver
	private float broadcastTimer = 0.0f;	// Timer for host broadcasting
	private List<HostInformation> hostList = null;      // List of hosts
    private float currentTime = 0.0f;   // Current time for UDP receiving
    private string gamePassword = "";   // Password for current game

    private string errorWindowTitle = "";   // Title on error window
    private string errorWindowText = null;    // Text on error window

    // GUI functions
    void OnGUI()
    {
        GUI.skin = this.guiSkin;
        this.currentTime = Time.time;

        switch(this.menuSelection)
        {


            case MainMenuSelection.MainMenu:
                if(GUI.Button(new Rect(10, 70, 200, 40), "Host Game"))
				{
                    this.menuSelection = MainMenuSelection.HostGame;
                    this.gamePassword = "";
				}

                if(GUI.Button(new Rect(10, 120, 200, 40), "Join Game"))
				{
                    this.menuSelection = MainMenuSelection.JoinGame;
                    this.hostList = new List<HostInformation>();
					this.receiver = new NetworkComponents.UDPReceiver(this.ReceiveUDPBroadcast);
                    this.connector = new NetworkComponents.TCPConnector(this.ReceiveTCPMessageGuest, false);
				}

                if(GUI.Button(new Rect(10, 170, 200, 40), "Exit"))
                    Application.Quit();

                break;



            case MainMenuSelection.HostGame:
			    GUI.Label(new Rect(10, 70, 500, 40), "Name: ");
                this.enteredName = GUI.TextField(new Rect(180, 70, 300, 40), this.enteredName);
			    GUI.Label(new Rect(10, 120, 500, 40), "Password: ");
                this.gamePassword = GUI.TextField(new Rect(180, 120, 300, 40), this.gamePassword);

                if(GUI.Button(new Rect(10, 170, 200, 40), "Create Game"))
                {
                    if (this.enteredName == null || this.enteredName.Length <= 0)
                    {
                        this.errorWindowText = "Please enter a name!";
                        this.errorWindowTitle = "Error";
                    }
                    else
                    {
                        this.menuSelection = MainMenuSelection.WaitForPlayers;
                        this.broadcaster = new NetworkComponents.UDPBroadcaster();
                        this.connector = new NetworkComponents.TCPConnector(this.ReceiveTCPMessageHost, true);
                    }
                }

                if(GUI.Button(new Rect(10, 220, 200, 40), "Return"))
				{
                    this.menuSelection = MainMenuSelection.MainMenu;
				}

                break;



            case MainMenuSelection.WaitForPlayers:
                this.broadcastTimer -= Time.deltaTime;
                if (this.broadcastTimer < 0.0f)
                {
                    bool passwordProtected = this.gamePassword != null && this.gamePassword.Length > 0;
                    HostInformation hostInformation = new HostInformation(this.broadcaster.GetHashCode().ToString(), NetworkComponents.GetLocalIPv4(), this.enteredName, Time.time, passwordProtected);
                    this.broadcaster.Broadcast((NetworkComponents.IGridforceMessage)(hostInformation));
                    this.broadcastTimer = 5.0f;
                }
                GUI.Label(new Rect(10, 70, 500, 40), "Your IP: " + NetworkComponents.GetLocalIPv4().ToString());

                if(GUI.Button(new Rect(10, 270, 200, 40), "Cancel"))
				{
                    this.menuSelection = MainMenuSelection.HostGame;
					this.broadcaster.Close();
                    this.connector.Close();
					this.broadcaster = null;
				}

                break;



            case MainMenuSelection.SetupGame:
                if (GUI.Button(new Rect(10, 170, 30, 40), "<"))
                {
                    int selectedArenaInt = (int)(this.selectedArena) - 1;
                    if (selectedArenaInt < 0)
                        selectedArenaInt = (int)(SelectedArena.Arena2);
                    this.selectedArena = (SelectedArena)(selectedArenaInt);
                }
                if (GUI.Button(new Rect(265, 170, 30, 40), ">"))
                {
                    this.selectedArena = (SelectedArena)(((int)(this.selectedArena) + 1) % ((int)(SelectedArena.Arena2) + 1));
                }
                GUI.Label(new Rect(45, 170, 200, 40), this.selectedArena.ToString());

                if(GUI.Button(new Rect(10, 220, 200, 40), "Start Game"))
                {
                    switch(this.selectedArena)
                    {
                        case SelectedArena.Arena1:
                            Application.LoadLevel(1);
                            break;
                        case SelectedArena.Arena2:
                            Application.LoadLevel(2);
                            break;
                    }
                }

                break;



            case MainMenuSelection.JoinGame:
                GUI.Label(new Rect(10, 70, 110, 40), "Host IP: ");
                this.enteredIP = GUI.TextField(new Rect(120, 70, 300, 40), this.enteredIP);

				int hostPos = 170;
				for(int i = 0; i < this.hostList.Count; i++)
                {
                    if (Time.time - this.hostList[i].lastUpdate >= 7.5f)
                    {
                        this.hostList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        GUI.Label(new Rect(10, hostPos, 500, 40), this.hostList[i].ToString());
                        hostPos += 50;
                    }
                }

                if(GUI.Button(new Rect(10, 120, 200, 40), "Return"))
                {
                    this.menuSelection = MainMenuSelection.MainMenu;
                    this.enteredIP = "";
                    this.receiver.Close();
                    this.connector.Close();
					this.receiver = null;
					this.hostList = null;
                }

                break;
        }

        if (this.errorWindowText != null)
            GUI.ModalWindow(0, new Rect(100, 100, 350, 350), this.ErrorWindowFunction, this.errorWindowTitle);
    }


    // Procedure for keeping track of user data
    public class UserData
    {
        public NetworkComponents.TCPConnection connection = null;
        public UserInformation userInformation = null;
        public bool verified = false;

        public UserData(NetworkComponents.TCPConnection connection, UserInformation userInformation = null)
        {
            this.connection = connection;
            this.userInformation = userInformation;
        }
    }



	// Procedure for receiving UDP broadcasts
	void ReceiveUDPBroadcast(NetworkComponents.IGridforceMessage message)
	{
        HostInformation hostInformation = message as HostInformation;
		if (hostInformation == null)
			return;
			
        hostInformation.lastUpdate = this.currentTime;

		if(hostInformation != null)
		{
			for(int i = 0; i < this.hostList.Count; i++)
			{
                if (this.hostList[i].Equals(hostInformation))
                {
                    this.hostList[i] = hostInformation;
                    return;
                }
			}
			this.hostList.Add(hostInformation);
		}
	}

    // Procedure for receiving TCP messages as host
    void ReceiveTCPMessageHost(NetworkComponents.TCPConnection connection, NetworkComponents.IGridforceMessage message)
    {
        if (message is JoinRequest)
        {
        }
    }

    // Procedure for receiving TCP messages as guest
    void ReceiveTCPMessageGuest(NetworkComponents.TCPConnection connection, NetworkComponents.IGridforceMessage message)
    {
    }

    // Function for error window
    void ErrorWindowFunction(int id)
    {
        GUI.Label(new Rect(10, 70, 500, 40), this.errorWindowText, this.guiSkin.customStyles[0]);
        if (GUI.Button(new Rect(10, 120, 200, 40), "OK"))
            this.errorWindowText = null;
    }
	
}
