using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArenaSettings : MonoBehaviour
{
    public float gridSize = 1.0f;
    public float maxDistance = 50.0f;
    public float minSpawnPointDistance = 10.0f;
    public List<Transform> spawnPoints = new List<Transform>();
    public Transform cameraTransform = null;      // Camera transform

	public float trailLengthMultiplyer = 1.0f;

    // Use this for initialization
	void Start ()
    {
        // Set tiling for all children
        MeshRenderer[] allChildren = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer child in allChildren)
        {
            if (child.gameObject.tag == "Drivable" || child.gameObject.tag == "NonDrivable")
            {
                child.material.mainTextureScale = new Vector2(child.gameObject.transform.localScale.x / this.gridSize, child.gameObject.transform.localScale.z / this.gridSize);
                child.material.SetTextureScale("_BumpMap", new Vector2(child.gameObject.transform.localScale.x / this.gridSize, child.gameObject.transform.localScale.z / this.gridSize));
            }
        }
	}

    // On drawing gizmos
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR

        MeshRenderer[] allChildren = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer child in allChildren)
        {
            if (child.gameObject.tag == "Drivable" || child.gameObject.tag == "NonDrivable")
            {
                child.sharedMaterial.mainTextureScale = new Vector2(child.gameObject.transform.localScale.x / this.gridSize, child.gameObject.transform.localScale.z / this.gridSize);
                child.sharedMaterial.SetTextureScale("_BumpMap", new Vector2(child.gameObject.transform.localScale.x / this.gridSize, child.gameObject.transform.localScale.z / this.gridSize));
            }
        }

        #endif
    }
}
