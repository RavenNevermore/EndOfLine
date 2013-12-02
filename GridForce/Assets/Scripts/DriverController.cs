using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
public class DriverController : MonoBehaviour
{
    public float baseSpeed = 10.0f;     // Base drive speed
    public float rotationSpeed = 10.0f;  // Rotation speed when changing orientation
    public ArenaSettings arenaSettings = null;      // Arena settings
    public Transform cameraTransform = null;    // The camera's transform
    public Vector3 cameraDistance = new Vector3(0, 2, -10);     // The camera's default position relative to the driver

    private CharacterController characterController;        // Character controller
    private Vector3 moveDirection;          // Character move direction
    private Vector3 gravityDirection;       // Character gravity
    private const int groundLayerMask = (1 << 8);       // Layer mask for ground
    private float gridSize = 1.0f;          // Grid size
    private List<PathNode> nodeList = new List<PathNode>();     // List of previous path nodes
    private PlayerAction playerAction = PlayerAction.None;      // Defines player's action
    private Vector3 cameraPos = Vector3.zero;         // The camera's current position relative to the driver


    // Defines a path node
    private struct PathNode
    {
        public Vector3 position;
        public Vector3 normal;

        public PathNode(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
    }


    // Defines player action
    private enum PlayerAction
    {
        None,
        TurnLeft,
        TurnRight,
        UseItem
    }


    // Finds next path node on player's path
    private PathNode GetNextNode()
    {
        PathNode pathNode = new PathNode(Vector3.zero, Vector3.up);

        RaycastHit raycastHit;
        if (Physics.Raycast(new Ray(this.transform.position, this.gravityDirection), out raycastHit, Mathf.Infinity, groundLayerMask))
        {
            // Convert from world to local space
            Vector3 localPos = raycastHit.transform.InverseTransformDirection(this.transform.position);
            Vector3 moveLocal = raycastHit.transform.InverseTransformDirection(this.moveDirection);

            // Convert to grid space
            localPos += raycastHit.transform.localScale * 0.5f;
            localPos.x += (0.5f + ((raycastHit.transform.localScale.x / this.gridSize) % 1.0f)) * this.gridSize;
            localPos.z += (0.5f + ((raycastHit.transform.localScale.z / this.gridSize) % 1.0f)) * this.gridSize;
            localPos.x /= this.gridSize;
            localPos.z /= this.gridSize;

            // Some rounding for safety...
            if (Math.Abs(moveLocal.x) < 0.9f)
                localPos.x = (float)(Math.Round(localPos.x, 0));
            else
                localPos.x = (float)(Math.Round(localPos.x, 1));
            if (Math.Abs(moveLocal.z) < 0.9f)
                localPos.z = (float)(Math.Round(localPos.z, 0));
            else
                localPos.z = (float)(Math.Round(localPos.z, 1));

            // Get next node
            localPos.x += (moveLocal.x - (localPos.x - (int)(localPos.x))) % 1.0f;
            localPos.z += (moveLocal.z - (localPos.z - (int)(localPos.z))) % 1.0f;

            // Convert back to local space
            localPos.x *= this.gridSize;
            localPos.z *= this.gridSize;
            localPos.x -= (0.5f + ((raycastHit.transform.localScale.x / this.gridSize) % 1.0f)) * this.gridSize;
            localPos.z -= (0.5f + ((raycastHit.transform.localScale.z / this.gridSize) % 1.0f)) * this.gridSize;
            localPos -= raycastHit.transform.localScale * 0.5f;

            Vector3 zeroLocal = raycastHit.transform.InverseTransformDirection(raycastHit.transform.position);

            localPos.y = zeroLocal.y;

            // Convert to world space
            pathNode.position = raycastHit.transform.TransformDirection(localPos);
            pathNode.normal = -this.gravityDirection;
        };

        return pathNode;
    }



	// Use this for initialization
	void Start()
    {
        this.characterController = this.gameObject.GetComponent<CharacterController>();
        if (this.cameraTransform != null)
            this.cameraPos = this.cameraDistance.x * this.transform.right + this.cameraDistance.y * this.transform.up + this.cameraDistance.z * this.transform.forward;
        this.moveDirection = this.transform.forward;
        this.gravityDirection = -this.transform.up;

        if (this.arenaSettings != null)
            this.gridSize = this.arenaSettings.gridSize;

        // Snap player to a close node point on grid
        RaycastHit raycastHit;
        if (Physics.Raycast(new Ray(this.transform.position, this.gravityDirection), out raycastHit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 localPos = raycastHit.transform.InverseTransformDirection(this.transform.position);

            localPos += raycastHit.transform.localScale * 0.5f;

            localPos /= this.gridSize;
            localPos.x = (int)(localPos.x) + 0.5f + ((raycastHit.transform.localScale.x / this.gridSize) % 1.0f);
            localPos.z = (int)(localPos.z) + 0.5f + ((raycastHit.transform.localScale.z / this.gridSize) % 1.0f);
            localPos *= this.gridSize;

            localPos -= raycastHit.transform.localScale * 0.5f;

            Vector3 nodePos = localPos;
            Vector3 zeroLocal = raycastHit.transform.InverseTransformDirection(raycastHit.transform.position);

            localPos.y = zeroLocal.y + this.characterController.radius + 0.1f;
            nodePos.y = zeroLocal.y;

            this.transform.position = raycastHit.transform.TransformDirection(localPos);
            nodePos = raycastHit.transform.TransformDirection(nodePos);

            this.nodeList.Add(new PathNode(nodePos, raycastHit.transform.up));
        };
	}


	
	// Update is called once per frame
	void Update()
    {
        for (int i = 0; i < this.nodeList.Count; i++)
            Debug.DrawRay(this.nodeList[i].position, this.nodeList[i].normal * 10.0f);

        Vector3 totalMovement = Vector3.zero;

        Vector3 gravityRayStart = this.transform.position - (this.moveDirection * (this.characterController.radius + 0.1f));
        Vector3 gravityRayEnd = gravityRayStart + (this.gravityDirection * (this.characterController.radius + 0.1f));

        // Get player action
        if (this.playerAction == PlayerAction.None)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.playerAction = PlayerAction.TurnLeft;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.playerAction = PlayerAction.TurnRight;
            }
        }

        // First check if player is in the air...
        if (!(Physics.Linecast(gravityRayStart, gravityRayEnd, groundLayerMask)))
        {
            // ...and if he is, check if there is a wall behind him
            if (Physics.Linecast(this.transform.position, this.transform.position - (this.moveDirection * (this.characterController.radius + 0.1f)), groundLayerMask))
            {
                // Change direction if a wall was detected
                Vector3 temp = -this.moveDirection;
                this.moveDirection = this.gravityDirection;
                this.gravityDirection = temp;
            }

            // Apply gravity
            totalMovement += this.gravityDirection * this.baseSpeed;
        }
        else
        {
            // Check, if player can turn
            PathNode nextNode = this.GetNextNode();
            Vector3 nodeOnDriver = nextNode.position; // this.nextNode.position;
            Quaternion transformRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.LookRotation(this.moveDirection, -this.gravityDirection);
            nodeOnDriver = this.transform.InverseTransformDirection(nodeOnDriver);
            nodeOnDriver.y = this.transform.InverseTransformDirection(this.transform.position).y;
            nodeOnDriver = this.transform.TransformDirection(nodeOnDriver);
            this.transform.rotation = transformRotation;

            float nodeDistance = (this.baseSpeed * this.moveDirection * Time.deltaTime).magnitude - (nodeOnDriver - this.transform.position).magnitude;

            if (nodeDistance >= 0)
            {
                switch (this.playerAction)
                {
                    case PlayerAction.TurnLeft:
                        this.moveDirection = Vector3.Cross(this.gravityDirection, this.moveDirection);
                        this.transform.position = nodeOnDriver - ((nodeOnDriver - this.transform.position).magnitude * this.moveDirection);
                        break;
                    case PlayerAction.TurnRight:
                        this.moveDirection = Vector3.Cross(this.moveDirection, this.gravityDirection);
                        this.transform.position = nodeOnDriver - ((nodeOnDriver - this.transform.position).magnitude * this.moveDirection);
                        break;
                    default:
                        break;
                }

                this.playerAction = PlayerAction.None;
                this.nodeList.Add(nextNode);
            }

            // If player is not in the air, check if there is a wall ahead of hif 
            if (Physics.Linecast(this.transform.position, this.transform.position + (this.moveDirection * (this.characterController.radius + 0.1f)), groundLayerMask))
            {
                // Change direction if a wall was detected
                Vector3 temp = this.moveDirection;
                this.moveDirection = -this.gravityDirection;
                this.gravityDirection = temp;
            }

            // Apply driving speed
            totalMovement += (this.baseSpeed * this.moveDirection);
        }

        // Player transformations
        totalMovement = new Vector3((float)(Math.Round(totalMovement.x, 1)), (float)(Math.Round(totalMovement.y, 1)), (float)(Math.Round(totalMovement.z, 1)));
        this.characterController.Move(totalMovement * Time.deltaTime);

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(this.moveDirection, -this.gravityDirection), this.rotationSpeed * Time.deltaTime);
        //this.transform.rotation = new Quaternion((float)(Math.Round(this.transform.rotation.x, 3)), (float)(Math.Round(this.transform.rotation.y, 3)), (float)(Math.Round(this.transform.rotation.z, 3)), (float)(Math.Round(this.transform.rotation.w, 3)));

        if (this.cameraTransform != null)
        {
            Vector3 targetPos = this.cameraDistance.x * this.transform.right + this.cameraDistance.y * this.transform.up + this.cameraDistance.z * this.transform.forward;
            this.cameraPos = Vector3.RotateTowards(this.cameraPos, targetPos, 0.1f, 10.0f);
            this.cameraTransform.position = this.transform.position + this.cameraPos;
            this.cameraTransform.rotation = Quaternion.LookRotation(-this.cameraPos, this.transform.up);
        }
     }
}
