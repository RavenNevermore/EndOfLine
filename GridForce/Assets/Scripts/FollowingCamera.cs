using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour {

	public Vector3 relativeCameraPosition = new Vector3(0, 1, -12);
	public Vector3 relativePositionInBoost = new Vector3(0, 1, -18);
	public Transform leader;
	public float speed = 0.15f;

	private bool inBoost;

	public void OnBoostStarted(){
		this.inBoost = true;
	}

	public void OnBoostEnded(){
		this.inBoost = false;
	}

	public void FollowTheLeader(Transform leader){
		this.leader = leader;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 wanted = this.calculateWantedPosition();
		wanted = this.restrictToBounds(wanted);
		this.moveTowards(wanted);

		this.LookAtLeader();
	}

	void LookAtLeader(){
		this.transform.LookAt(this.leader, this.leader.up);
	}

	Vector3 calculateWantedPosition(){
		Vector3 relativePos = this.inBoost ? this.relativePositionInBoost : this.relativeCameraPosition;
		Vector3 rotatedRelative = this.leader.rotation * relativePos;
		Vector3 relative = this.leader.position + rotatedRelative;
		return relative;
	}

	Vector3 restrictToBounds(Vector3 position){
		//TODO restrict camera to stay within the arena.
		return position;
	}

	void moveTowards(Vector3 target){
		this.transform.position = Vector3.Lerp(
			this.transform.position, 
			target,
			this.speed);
	}

	void OnTriggerEnter(Collider other){
		Debug.Log("Trigger: "+other);
	}
}
