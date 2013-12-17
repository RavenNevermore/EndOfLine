using UnityEngine;
using System.Collections;

public class RadarRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float delta = Time.deltaTime;
        this.transform.Rotate(
            Vector3.up, 135 * delta);
	}
}
