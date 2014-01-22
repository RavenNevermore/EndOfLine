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
        this.errorState = GameObject.Find("ErrorState").GetComponent<ErrorState>();

		Debug.Log("My menustate is : " + menuState);

		UdpBroadcasting.createBeacon();

		Input.simulateMouseWithTouches = true;

		TextMesh text = this.GetComponentInChildren<TextMesh>();

		text.text = "Your IP is " + Network.player.ipAddress + ".\nStart Game!";
	}

	void OnMouseDown()
    {
        if (!(this.menuState.AllPlayersReady()))
        {
            this.errorState.ClearButtons();
            this.errorState.AddLine("Some players not ready...", false);
            this.errorState.Show(3.0f);

            return;
        }

        Network.maxConnections = Network.connections.Length;

		GameObject gameState = null;
        if (Network.connections.Length > 0)
            gameState = (GameObject) UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
        else
			gameState = (GameObject) UnityEngine.Object.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity);

        GameObject[] itemBoxes = GameObject.FindGameObjectsWithTag("ItemBox");
        foreach (GameObject itemBox in itemBoxes)
        {
            ItemBoxBehavior itemBoxScript = itemBox.GetComponent<ItemBoxBehavior>();
            if (itemBoxScript != null)
                itemBoxScript.ReInstantiate();
        }

		UdpBroadcasting.destroyBeacon();

		gameState.GetComponent<GameState>().menuState = this.menuState;
        this.menuState.gameStarted = true;
	}
}
