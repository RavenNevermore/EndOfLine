using UnityEngine;
using System.Collections;

public class PulseOnTime : MonoBehaviour {

	public float minValue;
	public float maxValue;
	public float speed;

	private float t;
	private float deltaValue;

	// Use this for initialization
	void Start () {
		this.t = 0;
		this.deltaValue = this.maxValue - this.minValue;
	}
	
	// Update is called once per frame
	void Update () {
		float delta = Time.deltaTime * this.speed;
		this.t += delta;

		float sinus = (Mathf.Sin(this.t) + 1) / 2;

		this.light.intensity = this.minValue + (sinus * this.deltaValue);
	}
}
