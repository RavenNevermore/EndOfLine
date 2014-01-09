using UnityEngine;
using System.Collections;

public class RadarRotate : MonoBehaviour {

	public Vector3 rotationsVector;
	public float speed = 1;
	
	// Update is called once per frame
	void Update () {
        float delta = Time.deltaTime * speed;
        this.transform.Rotate(
            this.rotationsVector, 135 * delta);
	}
}
