using UnityEngine;
using System.Collections;

public class RotateAroundObject : MonoBehaviour {

    public float speed = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.up, 45 * Time.deltaTime * this.speed);
	}
}
