using UnityEngine;
using System.Collections;

public class RadarPing : MonoBehaviour {
	
	public float pauseTime = 2.0f;
	
	float deltaT = 0.0f;
	
	// Update is called once per frame
	void Update () {
		this.deltaT += Time.deltaTime;
		if (this.deltaT >= this.pauseTime){
			this.audio.Play();
			this.deltaT = 0.0f;
		}
	}
}
