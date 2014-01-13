using UnityEngine;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public GameObject gameStatePrefab = null;

	void Start ()
    {
		Input.simulateMouseWithTouches = true;

		//MenuState menuState = GameObject.Find("Arena").GetComponent<MenuState>();
		TextMesh text = this.GetComponentInChildren<TextMesh>();

		text.text = "Your IP is " + Network.player.ipAddress + ".\nStart Game!"; 
	}

	void OnMouseDown()
    {
        if (Network.connections.Length > 0)
            UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
        else
            UnityEngine.Object.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity);
	}
}
