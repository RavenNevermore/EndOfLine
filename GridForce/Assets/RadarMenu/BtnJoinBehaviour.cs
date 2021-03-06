﻿using UnityEngine;
using System;
using System.Collections;

public class BtnJoinBehaviour : AbstractMenuBehaviour
{	
	public Camera menuCamera;
	public string hostName;
	public string hostIp;
    public int otherPlayers;
    public ErrorState errorState = null;
    private MenuState menuState = null;
    private bool connecting = false;
    private GameObject parentObject = null;

	void Start()
    {
        this.errorState = GameObject.Find("ErrorState").GetComponent<ErrorState>();

        this.parentObject = this.transform.parent.gameObject;

        this.menuState = GameObject.Find("MenuState").GetComponent<MenuState>();

		this.resetName();
	}

	public void resetName()
    {
		this.resetName(this.hostName, this.hostIp);
	}

	public void resetName(string hostName, string hostIp)
    {
		this.hostIp = hostIp;
		this.hostName = hostName;
		if (null == this.hostName || this.hostName.Trim().Equals(""))
			this.hostName = hostIp;

		TextMesh text = this.GetComponentInChildren<TextMesh>();
		text.text = this.hostName /*+ "(" + this.otherPlayers + ")"*/;

	}
	
	
	void OnMouseDown()
    {
        if (!(this.connecting))
        {
            this.gameState.type = MenuState.GameType.JOIN;

            this.gameState.hostName = this.hostName;

            this.gameState.hostIp = this.hostIp;

            this.errorState.Clear();
            this.errorState.AddLine("Connecting to " + this.hostIp + "...", false);
            this.errorState.AddButton("Cancel", this.OnAbortedConnection);
            this.errorState.Show();

            this.transform.parent = this.transform.parent.parent.parent;
            if (this.parentObject.transform.parent != null)
                this.parentObject.transform.parent.gameObject.SetActive(false);

            this.connecting = true;
            this.menuState.ConnectAsClient();
        }
	}

    void OnConnectedToServer()
    {
        this.connecting = false;

        this.parentObject.transform.parent.gameObject.SetActive(true);
        this.transform.parent = this.parentObject.transform;

        this.errorState.AddLine("Connected to " + this.hostIp, false);
        this.errorState.ClearButtons();
        this.errorState.Show(3.0f);

        this.switchToMenu("03_select_vehicle");

        this.gameState.networkView.RPC("SetClientName", RPCMode.Server, Network.player, this.gameState.playerName);
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        this.errorState.AddLine("Failed to connect to " + this.hostIp, true);
        this.errorState.ClearButtons();
        this.errorState.Show(3.0f);
        this.OnAbortedConnection();

        this.connecting = false;
    }

    void OnAbortedConnection()
    {
        this.connecting = false;

        this.parentObject.transform.parent.gameObject.SetActive(true);
        this.transform.parent = this.parentObject.transform;

        this.switchToMenu("");
        this.switchToMenu("01_select_gamemode");

        Network.Disconnect(200);
    }
}
