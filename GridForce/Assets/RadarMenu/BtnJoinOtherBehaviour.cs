using UnityEngine;
using System;
using System.Net;
using System.Collections;

public class BtnJoinOtherBehaviour : AbstractMenuBehaviour
{

	public int boxWidth;
	public int boxHeight;
	public int borderWidth;
    public Color guiBackground;
    private MenuState menuState = null;
    private GameObject parentObject = null;
    public GUISkin guiSkin = null;
    public ErrorState errorState = null;

	Rect guiPosition;
	Rect textFieldPosition;
	Rect btnOkPosition;
	Rect btnEscPosition;
    bool enableGui;
    private bool connecting = false;

	public string ipToJoin = "";

	void Start()
    {
        this.errorState = GameObject.Find("ErrorState").GetComponent<ErrorState>();

        this.parentObject = this.transform.parent.gameObject;

        this.menuState = GameObject.Find("MenuState").GetComponent<MenuState>();

		this.guiPosition = new Rect(
			Screen.width / 2 - this.boxWidth / 2,
			Screen.height / 2 - this.boxHeight / 2,
			this.boxWidth,
			this.boxHeight);

		this.textFieldPosition = new Rect(
			this.guiPosition.x + this.borderWidth,
			this.guiPosition.y + this.guiPosition.height / 3,
			this.guiPosition.width - 2 * this.borderWidth,
			this.guiPosition.height / 3);

		this.btnOkPosition = new Rect(
			this.guiPosition.x + this.borderWidth,
			this.guiPosition.y + (this.guiPosition.height / 3) * 2,
			(this.guiPosition.width - 2 * this.borderWidth) / 2,
			this.guiPosition.height / 3);

		this.btnEscPosition = new Rect(
			this.guiPosition.x + this.borderWidth * 2 + this.btnOkPosition.width,
			this.guiPosition.y + (this.guiPosition.height / 3) * 2,
			(this.guiPosition.width - 2 * this.borderWidth) / 2,
			this.guiPosition.height / 3);

		this.enableGui = false;
	}

	void OnMouseDown()
    {
        if (this.enableGui || this.connecting)
            return;

        this.transform.parent = this.transform.parent.parent;
        this.parentObject.SetActive(false);
		this.enableGui = true;
	}

	void OnGUI()
    {
        GUI.skin = this.guiSkin;

		if (!this.enableGui || this.connecting)
			return;

		Event e = Event.current;
		
		if (e.keyCode == KeyCode.Return)
        {
			this.TryToConnect();
		}

		GUI.backgroundColor = this.guiBackground;

		GUI.Box(this.guiPosition, "Please enter the host's IP address:");

        GUI.SetNextControlName("IP Textfield");
		this.ipToJoin = GUI.TextField(this.textFieldPosition, this.ipToJoin, 15);

		if (GUI.Button(this.btnOkPosition, "OK"))
			this.TryToConnect();

		if (GUI.Button(this.btnEscPosition, "Cancel"))
        {
            this.parentObject.SetActive(true);
            this.transform.parent = this.parentObject.transform;
			this.enableGui = false;
			this.ipToJoin = "";
		}

        GUI.FocusControl("IP Textfield");
	}

	void TryToConnect()
    {
        if (!(this.connecting))
        {
            Debug.Log("Joining " + this.ipToJoin);

            this.gameState.type = MenuState.GameType.JOIN;

            this.gameState.hostName = "unknown";

            this.gameState.hostIp = this.ipToJoin;

            try
            {
                this.gameState.hostIp = IPAddress.Parse(this.gameState.hostIp).ToString();
            }
            catch (Exception)
            {
                this.gameState.hostIp = "0.0.0.0";
            }

            if (this.gameState.hostIp.Contains(":"))
                this.gameState.hostIp = "0.0.0.0";

            this.errorState.Clear();
            this.errorState.AddLine("Connecting to " + this.gameState.hostIp + "...", false);
            this.errorState.AddButton("Cancel", this.OnAbortedConnection);
            this.errorState.Show();

            this.connecting = true;
            this.menuState.ConnectAsClient();
        }
	}

    void OnConnectedToServer()
    {
        this.connecting = false;

        this.parentObject.SetActive(true);
        this.transform.parent = this.parentObject.transform;

        this.errorState.AddLine("Connected to " + this.gameState.hostIp, false);
        this.errorState.ClearButtons();
        this.errorState.Show(3.0f);

        this.enableGui = false;
        this.ipToJoin = "";

        this.switchToMenu("03_select_vehicle");

        this.gameState.networkView.RPC("SetClientName", RPCMode.Server, Network.player, this.gameState.playerName);
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        this.errorState.AddLine("Failed to connect to " + this.gameState.hostIp, true);
        this.errorState.ClearButtons();
        this.errorState.Show(3.0f);
        this.OnAbortedConnection();

        this.connecting = false;
    }

    void OnAbortedConnection()
    {
        this.connecting = false;

        this.parentObject.SetActive(true);
        this.transform.parent = this.parentObject.transform;

        this.enableGui = false;
        this.switchToMenu("");
        this.switchToMenu("01_select_gamemode");

        Network.Disconnect(200);
    }
}
