using UnityEngine;
using System;
using System.Collections;

public class BtnJoinOtherBehaviour : AbstractMenuBehaviour {

	public int boxWidth;
	public int boxHeight;
	public int borderWidth;
    public Color guiBackground;
    private MenuState menuState = null;
    public GameObject notificationPrefab = null;
    private GameObject notificationInstance = null;
    private GameObject parentObject = null;

	Rect guiPosition;
	Rect textFieldPosition;
	Rect btnOkPosition;
	Rect btnEscPosition;
    bool enableGui;
    private bool connecting = false;

	string ipToJoin;

	void Start()
    {
        this.parentObject = this.transform.parent.gameObject;

        this.menuState = GameObject.Find("state").GetComponent<MenuState>();

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

		this.ipToJoin = "";
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
		if (!this.enableGui || this.connecting)
			return;

		Event e = Event.current;
		
		if (e.keyCode == KeyCode.Return)
        {
			this.TryToConnect();
		}

		GUI.backgroundColor = this.guiBackground;

		GUI.Box(this.guiPosition, "Enter the Ip of a Host.");

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
	}

	void TryToConnect()
    {
        if (!(this.connecting))
        {
            try
            {
                UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.notificationPrefab, Vector3.zero, Quaternion.identity);
                this.notificationInstance = ((GameObject)(newObject));
                this.notificationInstance.GetComponentInChildren<GUIText>().text = "CONNECTING TO SERVER...";
            }
            catch (Exception)
            {
            }

            this.connecting = true;
            this.menuState.ConnectAsClient();
        }
	}

    void OnConnectedToServer()
    {
        this.connecting = false;

        this.parentObject.SetActive(true);
        this.transform.parent = this.parentObject.transform;

        UnityEngine.Object.Destroy(this.notificationInstance);
        this.notificationInstance = null;

        Debug.Log("Joining " + this.ipToJoin);

        this.gameState.type = MenuState.GameType.JOIN;

        this.gameState.hostName = "unknown";

        this.gameState.hostIp = this.ipToJoin;

        this.enableGui = false;
        this.ipToJoin = "";

        this.switchToMenu("03_select_vehicle");
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        UnityEngine.Object.Destroy(this.notificationInstance);
        this.notificationInstance = null;
        this.connecting = false;
    }
}
