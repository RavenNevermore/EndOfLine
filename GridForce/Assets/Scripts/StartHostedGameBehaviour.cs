using UnityEngine;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
	public GameObject gameState;

	void Start () {
		Input.simulateMouseWithTouches = true;
	}

	void OnMouseDown(){
		this.previewState.SetActive(false);
		this.gameState.SetActive(true);
	}
}
