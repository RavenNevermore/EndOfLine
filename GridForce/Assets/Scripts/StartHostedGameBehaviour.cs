using UnityEngine;
using System;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public GameObject gameStatePrefab = null;
    public GameObject notificationPrefab = null;
    public MenuState menuState = null;

	void Start ()
    {
		Debug.Log("My menustate is : " + menuState);

		Input.simulateMouseWithTouches = true;

		TextMesh text = this.GetComponentInChildren<TextMesh>();

		text.text = "Your IP is " + Network.player.ipAddress + ".\nStart Game!"; 

	}

	void OnMouseDown()
    {
        if (!(this.menuState.AllPlayersReady()))
        {
            try
            {
                UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.notificationPrefab, Vector3.zero, Quaternion.identity);
                ((GameObject)(newObject)).GetComponentInChildren<GUIText>().text = "NOT ALL PLAYERS ARE READY YET...";
                DontDestroyOnLoad(newObject);
            }
            catch (Exception)
            {
            }

            return;
        }

		GameObject gameState = null;
        if (Network.connections.Length > 0)
            gameState = (GameObject) UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
        else
			gameState = (GameObject) UnityEngine.Object.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity);

		gameState.GetComponent<GameState>().menuState = this.menuState;
        this.menuState.gameStarted = true;
	}
}
