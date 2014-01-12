using UnityEngine;
using System.Collections;

public class StartHostedGameBehaviour : MonoBehaviour {

	public GameObject previewState;
    public GameObject gameStatePrefab = null;

	void Start ()
    {
		Input.simulateMouseWithTouches = true;
	}

	void OnMouseDown()
    {
        UnityEngine.Network.Instantiate(this.gameStatePrefab, Vector3.zero, Quaternion.identity, 0);
	}
}
