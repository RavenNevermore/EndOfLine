using UnityEngine;
using System;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public GameObject gameStatePrefab = null;
    public MenuState menuState = null;
    public ErrorState errorState = null;

	void Start ()
    {
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            this.errorState = errorStateObject.GetComponent<ErrorState>();

		Debug.Log("My menustate is : " + this.menuState);

		if (MenuState.GameType.HOST == this.menuState.type){
	        try {
	            UdpBroadcasting.createBeacon();
				Debug.Log("Beacon is active!");
	        } catch (Exception e) {
				Debug.LogException(e);
	        }
		}

		Input.simulateMouseWithTouches = true;

		TextMesh text = this.GetComponentInChildren<TextMesh>();

		text.text = "Your IP is " + Network.player.ipAddress + ".\nStart Game!";
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

        // Reinstantiate all objects if necessary
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

		gameState.GetComponent<GameState>().menuState = this.menuState;
        this.menuState.gameStarted = true;
	}
}
