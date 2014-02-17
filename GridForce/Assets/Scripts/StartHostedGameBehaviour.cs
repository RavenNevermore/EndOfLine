using UnityEngine;
using System;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public GameObject gameStatePrefab = null;
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
        if (!(this.menuState.AllPlayersReady()))
        {
			this.errorState.showErrorMessage("Some players not ready...");
            return;
        }

        Network.maxConnections = Network.connections.Length;

		GameObject gameState = null;
        if (Network.connections.Length > 0)
            gameState = (GameObject) UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
        else
			gameState = (GameObject) UnityEngine.Object.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity);

        this.ReinstatiateObjectsIfNeccesary();
		
		

		gameState.GetComponent<GameState>().menuState = this.menuState;
        this.menuState.gameStarted = true;
	}
	
	void ReinstatiateObjectsIfNeccesary(){
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

		if (MenuState.GameType.HOST == this.menuState.type){
	        try {
	            UdpBroadcasting.destroyBeacon();
				Debug.Log("Beacon has been taken down!");
			} catch (Exception e) {
				Debug.LogException(e);
	        }
		}
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
