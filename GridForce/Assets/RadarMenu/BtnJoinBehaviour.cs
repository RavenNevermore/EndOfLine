using UnityEngine;
using System;
using System.Collections;

public class BtnJoinBehaviour : AbstractMenuBehaviour
{	
	public Camera menuCamera;
	public string hostName;
	public string hostIp;
	public int otherPlayers;
    private MenuState menuState = null;
    private bool connecting = false;
    public GameObject notificationPrefab = null;
    private GameObject notificationInstance = null;
    private GameObject parentObject = null;

	void Start()
    {
        this.parentObject = this.transform.parent.gameObject;

        this.menuState = GameObject.Find("MenuState").GetComponent<MenuState>();

		TextMesh text = this.GetComponentInChildren<TextMesh>();
		text.text = this.hostName + "(" + this.otherPlayers + ")";
	}
	
	
	void OnMouseDown()
    {
        if (!(this.connecting))
        {
            this.gameState.type = MenuState.GameType.JOIN;

            this.gameState.hostName = this.hostName;

            this.gameState.hostIp = this.hostIp;

            try
            {
                UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.notificationPrefab, Vector3.zero, Quaternion.identity);
                this.notificationInstance = ((GameObject)(newObject));
                this.notificationInstance.GetComponentInChildren<GUIText>().text = "CONNECTING TO SERVER...";
            }
            catch (Exception)
            {
            }

            this.transform.parent = this.transform.parent.parent;
            this.parentObject.SetActive(false);

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

        this.switchToMenu("03_select_vehicle");
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        UnityEngine.Object.Destroy(this.notificationInstance);
        this.notificationInstance = null;
        this.connecting = false;
    }
}
