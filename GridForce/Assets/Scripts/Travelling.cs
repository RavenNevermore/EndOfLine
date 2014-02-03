using UnityEngine;
using System.Collections;

public class Travelling : MonoBehaviour {

	public Transform[] pathNodes;
	public float speed;
	public float sloppyness;

	private int nextNodeIndex;

	void Start () {
		this.nextNodeIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
		Transform node = this.pathNodes[this.nextNodeIndex];

		Vector3 direction = node.position - this.transform.position;
		Vector3 travel = direction.normalized * speed * Time.deltaTime;

		this.transform.Translate(travel);

		float distance = Vector3.Distance(node.position, 
		                                  this.transform.position);

		//Debug.Log("--> " + distance);
		if (this.sloppyness > distance){

			this.nextNodeIndex = (this.nextNodeIndex + 1) % this.pathNodes.Length;
		}
	}
}
