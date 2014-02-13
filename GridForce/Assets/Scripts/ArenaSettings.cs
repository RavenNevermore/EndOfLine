using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArenaSettings : MonoBehaviour
{
	
	public float BaseScore = 2.0f;
    public float gridSize = 1.0f;
    public float maxDistance = 50.0f;
    public float minSpawnPointDistance = 10.0f;
    public List<Transform> spawnPoints = new List<Transform>();
    public Transform cameraTransform = null;      // Camera transform

	public float trailLengthMultiplyer = 1.0f;

    // Use this for initialization
	void Start ()
    {
        ArenaSettings.SetTextureScales(this.gameObject, this.gridSize);
	}

    // On drawing gizmos
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR

        //ArenaSettings.SetTextureScales(this.gameObject, this.gridSize);

        #endif
    }

    // Set tiling for all children in a game object
    public static void SetTextureScales(GameObject gameObject, float gridSize)
    {
        MeshRenderer[] allChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer child in allChildren)
        {
            if (child.gameObject.tag == "Drivable" || child.gameObject.tag == "NonDrivable")
            {
                child.material.mainTextureScale = new Vector2(child.gameObject.transform.localScale.x / gridSize, child.gameObject.transform.localScale.z / gridSize);
                child.material.SetTextureScale("_BumpMap", new Vector2(child.gameObject.transform.localScale.x / gridSize, child.gameObject.transform.localScale.z / gridSize));
                if (child.material.HasProperty("_GlowTex"))
                    child.material.SetTextureScale("_GlowTex", new Vector2(child.gameObject.transform.localScale.x / gridSize, child.gameObject.transform.localScale.z / gridSize));
                child.material.shaderKeywords = child.sharedMaterial.shaderKeywords;
            }
        }
    }
}
