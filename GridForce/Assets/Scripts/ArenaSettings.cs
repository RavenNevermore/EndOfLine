using UnityEngine;
using System.Collections;

public class ArenaSettings : MonoBehaviour
{
    public float tileRatio = 1.0f;

	// Set tiling for all children
	void Start ()
    {
        MeshRenderer[] allChildren = this.GetComponentsInChildren <MeshRenderer>();
        foreach (MeshRenderer child in allChildren)
        {
            child.material.mainTextureScale = new Vector2(child.gameObject.transform.localScale.x * tileRatio, child.gameObject.transform.localScale.y * tileRatio);
        }
	
	}
}
