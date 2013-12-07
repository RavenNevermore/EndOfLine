using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
public class DriverController : MonoBehaviour
{
    public Transform trailCollisionSegment = null;    // Game object that represents collision segment
    public Transform explosionPrefab = null;     // Which effect to use for the explosion
    public ArenaSettings arenaSettings = null;      // Arena settings
    public Transform cameraTransform = null;    // The camera's transform
    public float baseSpeed = 20.0f;     // Base drive speed
    public float baseTrailLength = 20.0f;   // Base length of trail
    public float rotationSpeed = 15.0f;  // Rotation speed when changing orientation
    public Vector3 cameraDistance = new Vector3(0.0f, 2.0f, -8.0f);     // The camera's default position relative to the driver
    public Vector3 cameraAngleShift = new Vector3(0.0f, 1.0f, 0.0f);      // The shift of camera angle after calculation
    public float swipeSpeed = 1500.0f;     // Speed of touch input swipe
    public Color mainColor = new Color(0.0f, 0.0f, 0.9f);    // Main color of light and trail

    private CharacterController characterController;        // Character controller
    private TimedTrailRenderer trailRenderer;      // This object's trail renderer
    private TrailRenderer cameraTrail;      // This object's trail renderer for the camera
    private Vector3 moveDirection;          // Character move direction
    private Vector3 gravityDirection;       // Character gravity
    private const int groundLayerMask = (1 << 8) | (1 << 9);       // Layer mask for ground
    private float gridSize = 1.0f;          // Grid size
    private List<PathNode> nodeList = new List<PathNode>();     // List of previous path nodes
    private PlayerAction playerAction = PlayerAction.None;      // Defines player's action
    private Vector3 cameraPos = Vector3.zero;         // The camera's current position relative to the driver
    private int fingerId = -1;      // Id of first finger touching screen
    private List<Transform> colliderList = new List<Transform>();   // List of colliders
    private bool killed = false;    // True when driver was killed


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

            Vector3 startOffset;
            startOffset.x = (raycastHit.transform.localScale.x - (this.gridSize * 0.5f)) % this.gridSize;
            startOffset.z = (raycastHit.transform.localScale.z - (this.gridSize * 0.5f)) % this.gridSize;

            // Convert to grid space
            localPos += raycastHit.transform.localScale * 0.5f;
            localPos.x -= startOffset.x;
            localPos.z -= startOffset.z;
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
            localPos.x += startOffset.x;
            localPos.z += startOffset.z;
            localPos -= raycastHit.transform.localScale * 0.5f;

            Vector3 zeroLocal = raycastHit.transform.InverseTransformDirection(raycastHit.transform.position);

            localPos.y = zeroLocal.y;

            // Convert to world space
            pathNode.position = raycastHit.transform.TransformDirection(localPos);
            pathNode.normal = -this.gravityDirection;
        };

        return pathNode;
    }


    // Update all colors to main color
    private void UpdateColors()
    {
        Light light = this.GetComponent<Light>();
        light.color = new Color(this.mainColor.r, this.mainColor.g, this.mainColor.b, light.color.a * this.mainColor.a);
        
        for (int i = 0; i < this.trailRenderer.colors.GetLength(0); i++)
            this.trailRenderer.colors[i] = new Color(this.mainColor.r, this.mainColor.g, this.mainColor.b, this.trailRenderer.colors[i].a * this.mainColor.a);
        this.cameraTrail.material.SetColor("_TintColor", new Color(this.mainColor.r, this.mainColor.g, this.mainColor.b, this.cameraTrail.material.GetColor("_TintColor").a * this.mainColor.a));
    }



    // Use this for initialization
    void Start()
    {
        this.characterController = this.gameObject.GetComponent<CharacterController>();
        if (this.cameraTransform != null)
            this.cameraPos = this.cameraDistance.x * this.transform.right + this.cameraDistance.y * this.transform.up + this.cameraDistance.z * this.transform.forward;
        this.moveDirection = this.transform.forward;
        this.gravityDirection = -this.transform.up;

        this.trailRenderer = this.GetComponentsInChildren<TimedTrailRenderer>()[0];
        this.trailRenderer.lifeTime = (this.baseTrailLength / this.baseSpeed) * this.gridSize;
        this.cameraTrail = this.GetComponentsInChildren<TrailRenderer>()[0];
        this.cameraTrail.time = (this.baseTrailLength / this.baseSpeed) * this.gridSize;

        if (this.arenaSettings != null)
            this.gridSize = this.arenaSettings.gridSize;

        this.UpdateColors();

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
        this.trailRenderer.lifeTime = (this.baseTrailLength / this.baseSpeed) * this.gridSize;
        this.cameraTrail.time = (this.baseTrailLength / this.baseSpeed) * this.gridSize;

        Vector3 totalMovement = Vector3.zero;

        Vector3 gravityRayStart = this.transform.position - (this.moveDirection * (this.characterController.radius + 0.1f));
        Vector3 gravityRayEnd = gravityRayStart + (this.gravityDirection * (this.characterController.height + 0.1f));

        // Get player action
        #if UNITY_STANDALONE || UNITY_EDITOR

        if (this.playerAction == PlayerAction.None)
        {
            if (Input.GetButtonDown("Left"))
            {
                this.playerAction = PlayerAction.TurnLeft;
            }
            else if (Input.GetButtonDown("Right"))
            {
                this.playerAction = PlayerAction.TurnRight;
            }
            else if (Input.GetButtonDown("Item"))
            {
                this.playerAction = PlayerAction.UseItem;
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                Application.Quit();
            }
        }

        #endif

        foreach (Touch touch in Input.touches)
        {
            if (this.fingerId >= 0 && touch.fingerId != this.fingerId)
                continue;

            if (touch.phase == TouchPhase.Moved && touch.deltaPosition.magnitude / touch.deltaTime >= this.swipeSpeed && this.fingerId != touch.fingerId)
            {
                float direction = 1000.0f;
                if (touch.deltaPosition.x != 0.0f)
                    direction = touch.deltaPosition.y / touch.deltaPosition.x;

                if (direction > -1.0f && direction < 1.0f)
                {
                    if (touch.deltaPosition.x < 0.0f)
                        this.playerAction = PlayerAction.TurnLeft;
                    else
                        this.playerAction = PlayerAction.TurnRight;
                }
                else if (touch.deltaPosition.y < 0.0f)
                    this.playerAction = PlayerAction.UseItem;

                this.fingerId = touch.fingerId;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled && this.fingerId != touch.fingerId)
            {
                this.fingerId = -1;
            }
        }

        // Player used item
        if (this.playerAction == PlayerAction.UseItem)
        {
            this.playerAction = PlayerAction.None;
        }

        // First check if player is in the air...
        if (!(Physics.Linecast(gravityRayStart, gravityRayEnd, groundLayerMask)))
        {
            // ...and if he is, check if there is a wall behind him
            if (Physics.Linecast(this.transform.position - (this.gravityDirection * (this.characterController.height / 2.0f)), this.transform.position - (this.moveDirection * (this.characterController.radius + 0.75f)), groundLayerMask))
            {
                // Change direction if a wall was detected
                this.nodeList.Add(new PathNode(this.transform.position, (-this.gravityDirection + this.moveDirection).normalized));

                Vector3 temp = -this.moveDirection;
                this.moveDirection = this.gravityDirection;
                this.gravityDirection = temp;
                this.characterController.Move(((this.characterController.radius + 0.1f) * this.moveDirection));
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

            Debug.DrawRay(nextNode.position, nextNode.normal * 10.0f, Color.yellow);

            float nodeDistance = (this.baseSpeed * this.moveDirection * Time.deltaTime).magnitude - (nodeOnDriver - this.transform.position).magnitude;

            if (nodeDistance >= 0 && (nextNode.position - this.transform.position).magnitude <= this.gridSize)
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

            // If player is not in the air, check if there is a wall ahead of him 
            if (Physics.Linecast(this.transform.position, this.transform.position + (this.moveDirection * (this.characterController.radius + 0.1f)), groundLayerMask))
            {
                // Change direction if a wall was detected
                this.nodeList.Add(new PathNode(this.transform.position, (-this.gravityDirection - this.moveDirection).normalized));

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

            targetPos = this.cameraAngleShift.x * this.transform.right + this.cameraAngleShift.y * this.transform.up + this.cameraAngleShift.z * this.transform.forward;
            this.cameraTransform.rotation = Quaternion.LookRotation(-this.cameraPos + targetPos, this.transform.up);
        }

        // Clean up node list
        this.nodeList.Add(new PathNode(this.transform.position, -this.gravityDirection));
        float currentLength = 0.0f;
        float totalLength = 0.0f;
        PathNode lastRemoved = new PathNode();

        for (int i = this.nodeList.Count - 2; i >= 0; i--)
        {
            currentLength += (this.nodeList[i].position - this.nodeList[i + 1].position).magnitude;
            if (currentLength > this.baseTrailLength * this.gridSize)
            {
                lastRemoved = this.nodeList[i];
                this.nodeList.RemoveAt(i);
            }
            if (currentLength <= this.baseTrailLength * this.gridSize)
                totalLength = currentLength;
        }

        Vector3 directionVector = (lastRemoved.position - this.nodeList[0].position).normalized;
        Vector3 normalVector = (this.nodeList[0].normal + lastRemoved.normal) * 0.5f;
        if (totalLength < this.baseTrailLength * this.gridSize)
        {
            float difference = ((this.baseTrailLength * this.gridSize) - totalLength) % this.gridSize;
            this.nodeList.Insert(0, new PathNode(this.nodeList[0].position + (difference * directionVector), normalVector));
        }

        // Create new trail collision
        int currentCollider = 0;
        for (int i = 0; i < this.nodeList.Count - 1; i++)
        {
            Vector3 lookDirection = this.nodeList[i + 1].position - this.nodeList[i].position;
            if (currentCollider >= this.colliderList.Count)
            {
                if (lookDirection.sqrMagnitude > 0)
                {
                    this.trailCollisionSegment.localScale = new Vector3(this.trailCollisionSegment.localScale.x, this.trailCollisionSegment.localScale.y, lookDirection.magnitude);
                    UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.trailCollisionSegment, this.nodeList[i].position + (lookDirection / 2.0f) + (this.nodeList[i].normal * (this.trailCollisionSegment.localScale.y / 2.0f)), Quaternion.LookRotation(lookDirection, this.nodeList[i].normal));
                    ((Transform)(newObject)).parent = this.transform;
                    this.colliderList.Add((Transform)(newObject));
                    currentCollider++;
                }
            }
            else
            {
                this.colliderList[i].localScale = new Vector3(this.trailCollisionSegment.localScale.x, this.trailCollisionSegment.localScale.y, lookDirection.magnitude);
                this.colliderList[i].position = this.nodeList[i].position + (lookDirection / 2.0f) + (this.nodeList[i].normal * (this.trailCollisionSegment.localScale.y / 2.0f));
                if (lookDirection.sqrMagnitude > 0)
                    this.colliderList[i].rotation = Quaternion.LookRotation(lookDirection, this.nodeList[i].normal);
                currentCollider++;
            }
        }

        this.nodeList.RemoveAt(this.nodeList.Count - 1);
    }


    // Kill driver
    void Kill()
    {
        if (!(this.killed))
        {
            this.mainColor = new Color(this.mainColor.r * 0.35f, this.mainColor.g * 0.35f, this.mainColor.b * 0.35f, this.mainColor.a * 0.1f);
            this.UpdateColors();

            this.trailRenderer.gameObject.transform.parent = null;
            this.cameraTrail.gameObject.transform.parent = null;

            UnityEngine.Object explosion = UnityEngine.Object.Instantiate(this.explosionPrefab, this.transform.position, this.transform.rotation);
            ParticleSystem particleSystem = ((Transform)(explosion)).gameObject.GetComponent<ParticleSystem>();
            particleSystem.startColor = this.mainColor;
            particleSystem = ((Transform)(explosion)).gameObject.GetComponentsInChildren<ParticleSystem>()[0];
            particleSystem.startColor = this.mainColor;
            UnityEngine.Object.Destroy(((Transform)(explosion)).gameObject, 3.0f);

            UnityEngine.Object.Destroy(this.trailRenderer.GetTrail(), this.trailRenderer.lifeTime);
            UnityEngine.Object.Destroy(this.trailRenderer.gameObject, this.trailRenderer.lifeTime);
            UnityEngine.Object.Destroy(this.cameraTrail.gameObject, this.cameraTrail.time);
            UnityEngine.Object.Destroy(this.gameObject);

            this.killed = true;
        }
    }


    // On collision enter
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == this.trailCollisionSegment.tag)
        {
            if (collider.transform.parent == this.transform)
            {
                float totalLength = 0;
                int lastIndex = this.nodeList.Count - 2;
                for (; lastIndex > 0 && totalLength < 2.0f; lastIndex--)
                {
                    totalLength += (this.nodeList[lastIndex].position - this.nodeList[lastIndex + 1].position).magnitude;
                }

                for (int i = lastIndex; i < this.colliderList.Count; i++)
                {
                    if (collider.transform == this.colliderList[i].transform)
                        return;
                }
            }

            this.Kill();
        }
    }
}
