using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BroadcastSailor : MonoBehaviour {

	public Transform radarLight;
	public Object beaconPrefab;
	public List<Vector3> positions;

	public float RefreshRate = 3;
	private float deltaT = 0.0f;
	private int positionIndex = 0;

	Dictionary<Beacon, GameObject> uiBeacons = new Dictionary<Beacon, GameObject>();

	void OnMenuActivation () {
		Debug.Log("Yay, we live!");
		this.positionIndex = 0;
		UdpBroadcasting.callAvailibleBeacons();
	}
	
	// Update is called once per frame
	void Update () {
		this.deltaT += Time.deltaTime;
		if (this.deltaT < this.RefreshRate){
			return;
		}
		this.deltaT = 0;


		HashSet<Beacon> beacons = UdpBroadcasting.availibleBeacons;
		HashSet<Beacon> toRemove = new HashSet<Beacon>();
		foreach (KeyValuePair<Beacon, GameObject> myBeacons in this.uiBeacons){
			if (!beacons.Contains(myBeacons.Key)){
				toRemove.Add(myBeacons.Key);
				myBeacons.Value.SetActive(false);
				Object.Destroy(myBeacons.Value);
			}
		}
		foreach (Beacon beacon in toRemove){
			this.uiBeacons.Remove(beacon);
		}

		foreach (Beacon beacon in beacons){
			if (null == beacon)
				continue;


			if (!this.uiBeacons.ContainsKey(beacon))
				this.uiBeacons.Add(beacon, this.createNewBeaconUi(beacon));
		}

		Debug.Log("*********************************");
	}

	Vector3 nextBeaconPosition(){
		int i = this.positionIndex++;
		this.positionIndex = this.positionIndex % this.positions.Count;
		return this.positions[i];
	}

	GameObject createNewBeaconUi(Beacon beacon){
		Vector3 position = this.nextBeaconPosition();

		GameObject beaconObj = (GameObject) Instantiate(beaconPrefab);
		beaconObj.transform.parent = this.transform;
		beaconObj.transform.localPosition = position;

		Quaternion rotation = new Quaternion(0, 0, 0, 0);
		beaconObj.transform.rotation = rotation;

		BtnJoinBehaviour behaviour = beaconObj.GetComponentInChildren<BtnJoinBehaviour>();
		if (null != behaviour){
			behaviour.resetName(beacon.name, beacon.ip);
		}

		return beaconObj;
	}

	void OnDisable(){
		UdpBroadcasting.killTheSailor();
		Debug.Log("I'm dead now!");
	}
}
