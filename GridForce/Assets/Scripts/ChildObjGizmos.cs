using UnityEngine;
using System.Collections;

public class ChildObjGizmos : MonoBehaviour {

    public Color color = Color.blue;

    void OnDrawGizmos() {
        Gizmos.color = this.color;

        Transform[] children = this.GetComponentsInChildren<Transform>();
        foreach (Transform child in children){
            Gizmos.DrawSphere(child.position, 0.5f);
        }
        
    }
}
