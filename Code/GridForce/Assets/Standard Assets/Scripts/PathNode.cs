using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Collider))]
public class PathNode : MonoBehaviour
{
    public List<Vector3> directions = new List<Vector3>();
    public Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);
    private const float lineLength = 10.0f;


    // Use this for initialization
    void Awake()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }


    // Draw gizmo
    void OnDrawGizmos()
    {
        this.OnDrawGizmosSelected();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));


        Vector3 direction = Vector3.zero;


        Gizmos.color = new Color(0, 1, 0, 1.0f);

        for (int i = 0; i < this.directions.Count; i++)
        {
            direction = this.directions[i];
            direction.Normalize();
            direction = this.transform.TransformDirection(direction) * PathNode.lineLength;
            Gizmos.DrawRay(this.transform.position, direction);
        }


        Gizmos.color = new Color(0, 0, 1, 1.0f);

        direction = this.upVector;
        direction.Normalize();
        direction = this.transform.TransformDirection(direction) * PathNode.lineLength;
        Gizmos.DrawRay(this.transform.position, direction);
    }
}