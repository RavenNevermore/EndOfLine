using UnityEngine;
using System.Collections;

public class DriverWallCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	

	public void OnControllerColliderHit(ControllerColliderHit hit){
		Debug.Log("STUFFFFFFFF!!!: "+hit.collider.gameObject.name);
	}
}
