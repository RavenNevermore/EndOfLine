using UnityEngine;
using System.Collections;

public class ArenaSettings : MonoBehaviour
{
    public float gridSize = 1.0f;

    // Use this for initialization
	void Start ()
    {
        // Set tiling for all children
        MeshRenderer[] allChildren = this.GetComponentsInChildren <MeshRenderer>();
        foreach (MeshRenderer child in allChildren)
        {
            child.material.mainTextureScale = new Vector2(child.gameObject.transform.localScale.x / this.gridSize, child.gameObject.transform.localScale.z / this.gridSize);
        }
	
	}
}
