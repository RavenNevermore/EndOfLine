using UnityEngine;
using System;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public MenuState menuState = null;
    public ErrorState errorState = null;

    public GameObject startGameButton = null;
    public GameObject connectionsList = null;
    public GameObject dummyButton = null;
    public GameObject ipAddress = null;

	void Start ()
    {
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            this.errorState = errorStateObject.GetComponent<ErrorState>();

		Debug.Log("My menustate is : " + this.menuState);

		Input.simulateMouseWithTouches = true;

		GUIText text = this.ipAddress.GetComponentInChildren<GUIText>();

		text.text = Network.player.ipAddress;
	}

	void OnMouseDown()
    {
        this.menuState.ProceedToControlSelection();
        this.HideAll();
	}

    public void HideAll()
    {
        this.startGameButton.SetActive(false);
        this.connectionsList.SetActive(false);
        this.dummyButton.SetActive(false);
        this.ipAddress.SetActive(false);
    }

    public void SetHostGame(MenuState menuState)
    {
        this.menuState = menuState;
        this.dummyButton.SetActive(false);
        this.gameObject.SetActive(true);
        this.StartUDPBroadcasting();
    }

    public void SetClientGame(MenuState menuState)
    {
        this.menuState = menuState;
        this.startGameButton.SetActive(false);
        this.gameObject.SetActive(true);

        GUIText text = this.ipAddress.GetComponentInChildren<GUIText>();
        text.text += "\n\nWaiting for host...";
    }

    void StartUDPBroadcasting()
    {
        if (MenuState.GameType.HOST == this.menuState.type)
        {
            try
            {
                UdpBroadcasting.createBeacon();
                Debug.Log("Beacon is active!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
