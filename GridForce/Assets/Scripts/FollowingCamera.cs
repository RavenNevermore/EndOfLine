using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour {

	public Vector3 relativeCameraPosition = new Vector3(0, 1, -12);
	public Transform leader;
	public float speed = 0.15f;



	public void FollowTheLeader(Transform leader){
		Debug.Log("Following "+leader);
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
		Vector3 rotatedRelative = this.leader.rotation * this.relativeCameraPosition;
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
}
