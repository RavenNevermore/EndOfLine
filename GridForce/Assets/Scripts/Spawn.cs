using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour {

	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 0.5f);
	}
}
