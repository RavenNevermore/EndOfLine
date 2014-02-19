using UnityEngine;
using System.Collections;

public class ScrollingPlane : MonoBehaviour
{
    public Material material;   // Line material
    public float scrollSpeed = 0.0025f;   // Scroll speed
    private Material instanceMaterial;          // The line material instance
    private Mesh planeMesh = null;               // The line mesh

    private Vector3[] meshVertices = new Vector3[] { new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.5f, -0.5f, 0.0f), new Vector3(-0.5f, -0.5f, 0.0f) };
    private int[] meshTriangles = new int[] { 0, 1, 2, 0, 2, 3 };
    private Vector3[] meshNormals = new Vector3[] { new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, -1.0f) };
    private Color[] meshColors = new Color[] { new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f) };
    private Vector2[] meshUVs = new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f) };

    public float timer = 0.0f;

	// Use this for initialization
	void Start ()
    {
        MeshFilter meshFilter = (MeshFilter)(this.gameObject.AddComponent(typeof(MeshFilter)));
        this.planeMesh = meshFilter.mesh;
        this.gameObject.AddComponent(typeof(MeshRenderer));
        this.instanceMaterial = UnityEngine.Object.Instantiate(this.material) as Material;
        this.instanceMaterial.shaderKeywords = this.material.shaderKeywords;
        this.gameObject.renderer.material = this.instanceMaterial;
	}
	
	// Update is called once per frame
	void Update ()
    {
        this.meshUVs[0].x = this.timer;
        this.meshUVs[1].x = this.timer + 1.0f;
        this.meshUVs[2].x = this.timer + 1.0f;
        this.meshUVs[3].x = this.timer;

        this.planeMesh.Clear();
        this.planeMesh.vertices = meshVertices;
        this.planeMesh.triangles = meshTriangles;
        this.planeMesh.normals = meshNormals;
        this.planeMesh.uv = meshUVs;
        this.planeMesh.colors = meshColors;

        this.timer = (this.timer + this.scrollSpeed) % 1.0f;
	}
}
