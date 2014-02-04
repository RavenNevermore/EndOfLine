using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptimizedLineRenderer : MonoBehaviour
{
    public Material material;   // Line material
    public List<OlrPoint> pointList = new List<OlrPoint>();     // List of line points
    public float baseLength = 1.0f;     // Line's base length

    private GameObject lineObject = null;       // The line game object
    private Mesh lineMesh = null;               // The line mesh
    private Material instanceMaterial;          // The line material instance

	// Use this for initialization
	void Start ()
    {
        this.lineObject = new GameObject("OLR Line");
        this.lineObject.transform.parent = null;
        this.lineObject.transform.position = Vector3.zero;
        this.lineObject.transform.rotation = Quaternion.identity;
        this.lineObject.transform.localScale = Vector3.one;

        MeshFilter meshFilter = (MeshFilter)(this.lineObject.AddComponent(typeof(MeshFilter)));
        this.lineMesh = meshFilter.mesh;
        this.lineObject.AddComponent(typeof(MeshRenderer));
        this.instanceMaterial = new Material(this.material);
        this.lineObject.renderer.material = this.instanceMaterial;	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (this.pointList.Count < 2)
            return;

        Vector3[] meshVertices = new Vector3[(this.pointList.Count * 8) + 8];
        int[] meshTriangles = new int[((this.pointList.Count - 1) * 24) + 12];
        Vector3[] meshNormals = new Vector3[(this.pointList.Count * 8) + 8];
        Vector2[] meshUVs = new Vector2[(this.pointList.Count * 8) + 8];
        Color[] meshColors = new Color[(this.pointList.Count * 8) + 8];


        Vector3 position = this.transform.position;
        Vector3 up = Vector3.zero;
        Vector3 forward = this.transform.forward;
        Vector3 right = Vector3.zero;
        Vector3 upNormal = this.transform.up;
        Vector3 rightNormal = this.transform.right;
        Vector3 forwardNormal = this.transform.forward;
        Vector3 lastUpNormal = upNormal;
        Vector3 lastRightNormal = rightNormal;
        Vector3 lastForwardNormal = forwardNormal;
        Vector3 lastForward = forward;
        Vector3 tempVector = Vector3.zero;
        Vector3 planeNormal = this.transform.forward;
        float currentLength = 0.0f;
        float xPosUV = 1.0f;
        int pointIndex = 0;

        position = this.pointList[0].position;
        up = this.pointList[0].normal * this.pointList[0].height;
        forward = this.pointList[1].position - position;
        if (forward == Vector3.zero)
            forward = lastForward;
        lastForward = forward;
        forwardNormal = forward.normalized;
        if (forwardNormal == Vector3.zero)
            forwardNormal = lastForwardNormal;
        lastForwardNormal = forwardNormal;
        right = Vector3.Cross(up, forward).normalized * this.pointList[0].width;

        tempVector = position + (right * 0.5f) + up;
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = -forwardNormal;
        meshColors[pointIndex] = this.pointList[0].color;
        meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
        pointIndex++;

        tempVector = position - (right * 0.5f) + up;
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = -forwardNormal;
        meshColors[pointIndex] = this.pointList[0].color;
        meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
        pointIndex++;

        tempVector = position + (right * 0.5f);
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = -forwardNormal;
        meshColors[pointIndex] = this.pointList[0].color;
        meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);
        pointIndex++;

        tempVector = position - (right * 0.5f);
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = -forwardNormal;
        meshColors[pointIndex] = this.pointList[0].color;
        meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);

        for (int i = 0; i < this.pointList.Count; i++)
        {
            if (i > 0)
                currentLength += (this.pointList[i].position - this.pointList[i - 1].position).magnitude;

            xPosUV = 1.0f - (currentLength / this.baseLength);

            position = this.pointList[i].position;
            up = this.pointList[i].normal * this.pointList[i].height;
            if (i == this.pointList.Count - 1)
                forward = -(this.pointList[i - 1].position - position);
            else
                forward = this.pointList[i + 1].position - position;
            if (forward == Vector3.zero)
                forward = lastForward;
            planeNormal = this.GetDiagonalPlaneNormal(forward, -lastForward);
            lastForward = forward;
            upNormal = up.normalized;
            if (upNormal == Vector3.zero)
                upNormal = lastUpNormal;
            lastUpNormal = upNormal;
            rightNormal = right.normalized;
            if (rightNormal == Vector3.zero)
                rightNormal = lastRightNormal;
            lastRightNormal = rightNormal;
            right = Vector3.Cross(up, forward).normalized * this.pointList[i].width * 0.5f;
            right = this.FindVertexPos(planeNormal, right);
            up = this.FindVertexPos(planeNormal, up);

            pointIndex = 4 + (i * 8);

            tempVector = position + right + up;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = upNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
            pointIndex++;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = rightNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
            pointIndex++;

            tempVector = position - right + up;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = upNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
            pointIndex++;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = -rightNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 1.0f);
            pointIndex++;

            tempVector = position + right;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = -upNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);
            pointIndex++;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = rightNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);
            pointIndex++;

            tempVector = position - right;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = -upNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);
            pointIndex++;
            meshVertices[pointIndex] = tempVector;
            meshNormals[pointIndex] = -rightNormal;
            meshColors[pointIndex] = this.pointList[i].color;
            meshUVs[pointIndex] = new Vector2(xPosUV, 0.0f);
        }

        pointIndex = meshVertices.GetLength(0) - 4;
        
        position = this.pointList[this.pointList.Count - 1].position;
        up = this.pointList[this.pointList.Count - 1].normal * this.pointList[this.pointList.Count - 1].height;
        forward = -(this.pointList[this.pointList.Count - 2].position - position);
        if (forward == Vector3.zero)
            forward = lastForward;
        lastForward = forward;
        forwardNormal = forward.normalized;
        if (forwardNormal == Vector3.zero)
            forwardNormal = lastForwardNormal;
        lastForwardNormal = forwardNormal;
        right = Vector3.Cross(up, forward).normalized * this.pointList[this.pointList.Count - 1].width;

        tempVector = position + (right * 0.5f) + up;
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = forwardNormal;
        meshColors[pointIndex] = this.pointList[this.pointList.Count - 1].color;
        meshUVs[pointIndex] = new Vector2(1.0f, 1.0f);
        pointIndex++;

        tempVector = position - (right * 0.5f) + up;
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = forwardNormal;
        meshColors[pointIndex] = this.pointList[this.pointList.Count - 1].color;
        meshUVs[pointIndex] = new Vector2(1.0f, 1.0f);
        pointIndex++;

        tempVector = position + (right * 0.5f);
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = forwardNormal;
        meshColors[pointIndex] = this.pointList[this.pointList.Count - 1].color;
        meshUVs[pointIndex] = new Vector2(1.0f, 0.0f);
        pointIndex++;

        tempVector = position - (right * 0.5f);
        meshVertices[pointIndex] = tempVector;
        meshNormals[pointIndex] = forwardNormal;
        meshColors[pointIndex] = this.pointList[this.pointList.Count - 1].color;
        meshUVs[pointIndex] = new Vector2(1.0f, 0.0f);


        int nextPointIndex = 0;
        int triIndex = 0;

        meshTriangles[triIndex] = 0;
        triIndex++;
        meshTriangles[triIndex] = 2;
        triIndex++;
        meshTriangles[triIndex] = 1;
        triIndex++;

        meshTriangles[triIndex] = 1;
        triIndex++;
        meshTriangles[triIndex] = 2;
        triIndex++;
        meshTriangles[triIndex] = 3;

        for (int i = 0; i < this.pointList.Count - 1; i++)
        {
            pointIndex = 4 + (i * 8);
            nextPointIndex = 4 + ((i + 1) * 8);
            triIndex = 6 + (i * 24);

            meshTriangles[triIndex] = pointIndex;
            triIndex++;
            meshTriangles[triIndex] = pointIndex + 2;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 2;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 2;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 1;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 1;
            triIndex++;
            meshTriangles[triIndex] = pointIndex + 5;
            triIndex++;

            meshTriangles[triIndex] = nextPointIndex + 1;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 5;
            triIndex++;
            meshTriangles[triIndex] = pointIndex + 5;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 3;
            triIndex++;
            meshTriangles[triIndex] = pointIndex + 7;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 7;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 3;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 7;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 3;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 4;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 4;
            triIndex++;
            meshTriangles[triIndex] = pointIndex + 6;
            triIndex++;

            meshTriangles[triIndex] = pointIndex + 6;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 4;
            triIndex++;
            meshTriangles[triIndex] = nextPointIndex + 6;
        }

        pointIndex = meshVertices.GetLength(0) - 4;
        triIndex = meshTriangles.GetLength(0) - 6;

        meshTriangles[triIndex] = pointIndex;
        triIndex++;
        meshTriangles[triIndex] = pointIndex + 1;
        triIndex++;
        meshTriangles[triIndex] = pointIndex + 2;
        triIndex++;

        meshTriangles[triIndex] = pointIndex + 1;
        triIndex++;
        meshTriangles[triIndex] = pointIndex + 3;
        triIndex++;
        meshTriangles[triIndex] = pointIndex + 2;

        this.lineObject.transform.position = Vector3.zero;
        this.lineObject.transform.rotation = Quaternion.identity;

        this.lineMesh.Clear();
        this.lineMesh.vertices = meshVertices;
        this.lineMesh.triangles = meshTriangles;
        this.lineMesh.normals = meshNormals;
        this.lineMesh.uv = meshUVs;
        this.lineMesh.colors = meshColors;
	}

    // Return normal of two vectors' diagonal separting plane
    private Vector3 GetDiagonalPlaneNormal(Vector3 forward1, Vector3 forward2)
    {
        Vector3 forward1Normal = forward1.normalized;
        Vector3 forward2Normal = forward2.normalized;
        float dotProduct = Vector3.Dot(forward1Normal, forward2Normal);
        if (dotProduct >= 1.0f || dotProduct <= -1.0f)
            return (forward1 + forward2).normalized;

        return Vector3.Cross(forward1Normal + forward2Normal, Vector3.Cross(forward1Normal, forward2Normal)).normalized;
    }

    // Find position of vertix
    private Vector3 FindVertexPos(Vector3 planeNormal, Vector3 vector)
    {
        Vector3 direction = vector - Vector3.Project(vector, planeNormal);
        float percentage = Vector3.Project(direction, vector).magnitude / vector.magnitude;
        return direction / percentage;
    }

    // Destroy line object when destroying object containing this script
    void OnDestroy()
    {
        UnityEngine.Object.Destroy(this.lineObject);
    }
}


public interface OlrPoint
{
    Vector3 position { get; set; }
    Vector3 normal { get; set; }
    float height { get; set; }
    float width { get; set; }
    Color color { get; set; }
}
