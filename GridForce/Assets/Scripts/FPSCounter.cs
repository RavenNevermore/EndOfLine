using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour {

	float fps = 0.0f;
	
	int frames = 0;
	
	// Update is called once per frame
	void Update () {
		this.fps = 1 / Time.smoothDeltaTime;
		
		if (30 <= frames++){
			frames = 0;
			Debug.Log("-- FPS: "+fps+" ---");
		}
	}
}
