﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
public class DriverController : ExtendedBehaviour
{
    public Transform trailCollisionSegment = null;    // Game object that represents collision segment
    public Transform itemBoxPrefab = null;      // Item box prefab
    public Transform explosionPrefab = null;     // Which effect to use for the explosion
	public string[] explosionParticlesNames; //which part of the explosion effect to color in.
    public GameObject fakeItemBoxPrefab = null;     // Fake item box
    public GameObject sideBladePrefab = null;       // Side blade
    public ArenaSettings arenaSettings = null;      // Arena settings
    public Transform cameraTransform = null;    // The camera's transform
    public DriverInput driverInput = null;    // Input for driver
    public float baseSpeed = 20.0f;     // Base drive speed
    public float boostSpeed = 30.0f;    // Speed when on boost
    public float boostDuration = 5.0f;    // Duration of boost
    public float immunityDuration = 10.0f;  // Immunity duration
    public float sideBladeDuration = 10.0f; // Duration of side blades
    public float killBoostDuration = 1.5f;  // Duration of kill boost
    public float baseTrailLength = 20.0f;   // Base length of trail
    public float rotationSpeed = 15.0f;  // Rotation speed when changing orientation
    public Vector3 cameraDistance = new Vector3(0.0f, 2.0f, -8.0f);     // The camera's default position relative to the driver
    public Vector3 cameraAngleShift = new Vector3(0.0f, 1.0f, 0.0f);      // The shift of camera angle after calculation
    public Color mainColor = new Color(0.0f, 0.0f, 0.9f);    // Main color of light and trail
    public bool updateColor = true;     // Whether to update player's color
    public int playerIndex = 0;    // This driver's player index
    public bool gameStarted = false;        // Has the game started yet?
    public ScoreDelegate ScoreFunction = null;
    public GameObject shieldMesh = null;        // Shield mesh

    private const int layerDrivable = 8;     // Layer of drivable plane
    private const int layerNonDrivable = 9;    // Layer of non drivable plane
    private const int drivableLayerMask = (1 << DriverController.layerDrivable);       // Layer mask for drivable planes
    private const int nonDrivableLayerMask = (1 << DriverController.layerNonDrivable);    // Layer mask for non-drivable planes
    private const int layerDriver = 10;     // Layer of driver
    private const int layerDriverInvincible = 12;    // Layer of invincible driver

    private CharacterController characterController;        // Character controller
    public OptimizedLineRenderer lineRenderer;      // This object's line renderer
    private OptimizedLineRenderer lineRendererKilled;      // Line renderer when killed
    private GameObject vehicleMesh;      // This object's mesh
    private Vector3 moveDirection;          // Character move direction
    private Vector3 gravityDirection;       // Character gravity
    private float gridSize = 1.0f;          // Grid size
    private List<OlrPoint> nodeList = new List<OlrPoint>();     // List of previous path nodes
    private PlayerAction playerAction = PlayerAction.None;      // Defines player's action
    private Vector3 cameraPos = Vector3.zero;         // The camera's current position relative to the driver
    private List<Transform> colliderList = new List<Transform>();   // List of colliders
    private bool killed = false;    // True when driver was killed
    private float invincibleTimer = 3.0f;       // Driver invincible
    private float spawnedTimer = 3.5f;      // Spawn timer
    private float harmlessTimer = 3.0f;         // Driver can't hurt other players
    private GameObject trailCollisionObject;    // Trail collisions' parent game object
    private float killTimer = 3.0f;     // Seconds until object is destroyed
    private float boostTime = 0.0f;     // Remaining time of boost
    private float currentSpeed = 0.0f;  // The current speed
    public ItemType heldItem = ItemType.None;  // The currently held item
    private bool removedNode = false;       // Node was removed this frame
    private bool insertedNode = false;      // Node was inserted this frame
    private float killedTrailLength = 0.0f;     // Length of trail when killed
    private float sideBladeTimer = 0.0f;        // Side blade timer
    private GameObject sideBladeOne = null;     // First side blade
    private GameObject sideBladeTwo = null;     // Second side blade

    public GameObject[] meshList = null;   // List of all meshes


    // Defines a path node
    public struct PathNode : OlrPoint
    {
        public Vector3 position { get { return this._position; } set { this._position = value; } }
        public Vector3 normal { get { return this._normal; } set { this._normal = value; } }
        public float height { get { return this._height; } set { this._height = value; } }
        public float width { get { return this._width; } set { this._width = value; } }
        public Color color { get { return this._color; } set { this._color = value; } }

        private Vector3 _position;
        private Vector3 _normal;
        private float _height;
        private float _width;
        private Color _color;
        public Vector3 nextNormal;

        public PathNode(Vector3 position, Vector3 normal, Vector3 nextNormal, Color color)
        {
            this._position = position;
            this._normal = normal;
            this.nextNormal = nextNormal;

            this._height = 1.0f;
            this._width = 0.05f;
            if (normal != nextNormal)
                this._height = new Vector3(this._height, this._height, 0).magnitude;
            this._color = color;
        }
    }


    // Finds next path node on player's path
    private PathNode GetNextNode()
    {
        PathNode pathNode = new PathNode(Vector3.zero, Vector3.up, Vector3.up, Color.grey);

        RaycastHit raycastHit;
        if (Physics.Raycast(new Ray(this.transform.position, this.gravityDirection), out raycastHit, Mathf.Infinity, DriverController.drivableLayerMask /*| DriverController.nonDrivableLayerMask*/))
        {
            // Convert from world to local space
            Vector3 localPos = raycastHit.transform.InverseTransformDirection(this.transform.position);
            Vector3 moveLocal = raycastHit.transform.InverseTransformDirection(this.moveDirection);

            Vector3 startOffset;
            startOffset.x = (raycastHit.transform.localScale.x - (this.gridSize * 0.5f)) % this.gridSize;
            startOffset.z = (raycastHit.transform.localScale.z - (this.gridSize * 0.5f)) % this.gridSize;

            Vector3 zeroShift = (raycastHit.transform.localScale * 0.5f) + Vector3.one;
            zeroShift.x = (int)(zeroShift.x);
            zeroShift.y = (int)(zeroShift.y);
            zeroShift.z = (int)(zeroShift.z);

            // Convert to grid space
            localPos += zeroShift;
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
            localPos -= zeroShift;

            Vector3 zeroLocal = raycastHit.transform.InverseTransformDirection(raycastHit.transform.position);

            localPos.y = zeroLocal.y;

            // Convert to world space
            pathNode.position = raycastHit.transform.TransformDirection(localPos);
            pathNode.normal = -this.gravityDirection;
            pathNode.nextNormal = -this.gravityDirection;
        };

        return pathNode;
    }


    // Update all colors to main color
    private void UpdateColors()
    {
        Color currentColor = this.mainColor;
        if (this.boostTime > 0.0f)
            currentColor = new Color(1.0f, 0.8f, 0.05f, currentColor.a);
        Color trailColor = currentColor;
        if (this.harmlessTimer > 0.0f && this.gameStarted)
        {
            trailColor = new Color(currentColor.r * 0.35f, currentColor.g * 0.35f, currentColor.b * 0.35f, currentColor.a * 0.0f);
            currentColor = new Color(currentColor.r * 0.35f, currentColor.g * 0.35f, currentColor.b * 0.35f, currentColor.a * 0.1f);
        }

        for (int i = 0; this.lineRenderer != null && i < this.lineRenderer.pointList.Count; i++)
        {
            this.lineRenderer.pointList[i].color = new Color(trailColor.r, trailColor.g, trailColor.b, trailColor.a);
        }

        if (this.lineRenderer != null && this.lineRenderer.instanceMaterial != null)
        {
            if (this.harmlessTimer > 0.0f)
                this.lineRenderer.instanceMaterial.SetFloat("_GlowStrength", 0.25f);
            else
                this.lineRenderer.instanceMaterial.SetFloat("_GlowStrength", 0.6f);
        }

        for (int i = 0; this.lineRendererKilled != null && this.lineRendererKilled.pointList != null && i < this.lineRendererKilled.pointList.Count; i++)
        {
            this.lineRendererKilled.pointList[i].color = new Color(trailColor.r, trailColor.g, trailColor.b, trailColor.a);
        }

        if (this.lineRendererKilled != null && this.lineRendererKilled.instanceMaterial != null)
        {
            if (this.harmlessTimer > 0.0f)
                this.lineRendererKilled.instanceMaterial.SetFloat("_GlowStrength", 0.25f);
            else
                this.lineRendererKilled.instanceMaterial.SetFloat("_GlowStrength", 0.6f);
        }

        if (this.vehicleMesh == null)
            return;

        MeshRenderer[] meshRenderers = this.vehicleMesh.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.materials[1].SetColor("_TintColor", currentColor);
            renderer.materials[1].SetColor("_GlowColor", currentColor);
        }
    }



    // Use this for initialization
    void Start()
    {
		this.baseTrailLength = this.baseTrailLength * this.arenaSettings.trailLengthMultiplyer;

        this.characterController = this.gameObject.GetComponent<CharacterController>();
        if (this.cameraTransform != null)
            this.cameraPos = this.cameraDistance.x * this.transform.right + this.cameraDistance.y * this.transform.up + this.cameraDistance.z * this.transform.forward;
        this.moveDirection = this.transform.forward;
        this.gravityDirection = -this.transform.up;

        this.lineRenderer = this.GetComponentInChildren<OptimizedLineRenderer>();
        this.vehicleMesh = this.transform.Find("VehicleMeshes").gameObject;

        if (this.arenaSettings != null)
            this.gridSize = this.arenaSettings.gridSize;

        this.trailCollisionObject = new GameObject("Trail Collision");
        TrailCollision trailCollision = this.trailCollisionObject.AddComponent<TrailCollision>();
        trailCollision.owner = this;

        this.UpdateColors();

        // Snap player to a close node point on grid
        RaycastHit raycastHit;
        if (Physics.Raycast(new Ray(this.transform.position, this.gravityDirection), out raycastHit, Mathf.Infinity, DriverController.drivableLayerMask /*| DriverController.nonDrivableLayerMask*/))
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

            this.nodeList.Add(new PathNode(nodePos, raycastHit.transform.up, raycastHit.transform.up, this.mainColor));
            this.nodeList.Add(new PathNode(nodePos, raycastHit.transform.up, raycastHit.transform.up, this.mainColor));
        };
	}


	
	// Update is called once per frame
    void Update()
    {
        if (this.vehicleMesh != null)
        {
            if (this.invincibleTimer > 0.0f)
            {
                this.invincibleTimer -= Time.deltaTime;
                if (this.gameStarted)
                {
                    if (this.invincibleTimer > 0.5f && this.spawnedTimer <= 0.0f)
                        this.shieldMesh.SetActive(true);
                    float modulo = this.invincibleTimer % 0.1f;
                    if (modulo > 0.05f)
                        this.vehicleMesh.SetActive(false);
                    else
                        this.vehicleMesh.SetActive(true);
                }

                if (this.spawnedTimer > 0.0f)
                    this.spawnedTimer -= Time.deltaTime;
            }
            else
                this.vehicleMesh.SetActive(true);
        }

        if (this.invincibleTimer <= 0.5f)
            this.shieldMesh.SetActive(false);

        if (this.harmlessTimer > 0.0f)
        {
            if (this.trailCollisionObject != null)
                this.trailCollisionObject.SetActive(false);
            this.harmlessTimer -= Time.deltaTime;
            this.updateColor = true;
        }
        else if (this.trailCollisionObject != null)
            this.trailCollisionObject.SetActive(true);

        if (this.updateColor)
        {
            this.UpdateColors();
            this.updateColor = false;
        }

        Color nodeColor = this.mainColor;
        if (this.nodeList.Count > 0)
            nodeColor = this.nodeList[0].color;

        Vector3 totalMovement = Vector3.zero;

        if (!(this.killed) && this.gameStarted)
        {
            this.killedTrailLength = this.baseTrailLength;

            if (this.nodeList.Count > 0)
                this.nodeList.RemoveAt(this.nodeList.Count - 1);

            this.insertedNode = false;
            this.removedNode = false;

            if (this.invincibleTimer > 0.0f)
                this.gameObject.layer = DriverController.layerDriverInvincible;
            else
                this.gameObject.layer = DriverController.layerDriver;
            
            Vector3 gravityRayStart = this.transform.position - (this.moveDirection * (this.characterController.radius + 0.1f));
            Vector3 gravityRayEnd = gravityRayStart + (this.gravityDirection * (this.characterController.height + 0.1f));

            // Get player action
            if (this.driverInput != null)
                this.playerAction = this.driverInput.playerAction;

            // Player used item
            if (this.playerAction == PlayerAction.UseItem)
            {
                this.playerAction = PlayerAction.None;
                if (this.driverInput != null)
                    this.driverInput.playerAction = PlayerAction.None;

                switch (this.heldItem)
                {
                    case ItemType.Boost:
                        this.boostTime = this.boostDuration;
						this.ShoutMessage("OnBoostStarted");
                        break;

                    case ItemType.FakeItemBox:
						this.SendMessage("OnMineCharged");
                        UnityEngine.Object fakeItemBoxInstance = null;
                        RaycastHit raycastHitFakeItemBox;
                        Physics.Linecast(gravityRayStart, gravityRayEnd, out raycastHitFakeItemBox, DriverController.drivableLayerMask | DriverController.nonDrivableLayerMask);
                        Vector3 fakeItemBoxPosition = raycastHitFakeItemBox.point - (this.moveDirection * 3.0f) - (this.gravityDirection * this.fakeItemBoxPrefab.transform.localScale.y * 0.5f);
                        if (Physics.Raycast(fakeItemBoxPosition, this.gravityDirection, out raycastHitFakeItemBox, Mathf.Infinity, DriverController.drivableLayerMask | DriverController.nonDrivableLayerMask))
                        {
                            fakeItemBoxPosition = raycastHitFakeItemBox.point - (this.moveDirection * 3.0f) - (this.gravityDirection * this.fakeItemBoxPrefab.transform.localScale.y * 0.5f);
                            if (Network.connections.Length > 0)
                                fakeItemBoxInstance = UnityEngine.Network.Instantiate(this.fakeItemBoxPrefab, fakeItemBoxPosition, Quaternion.LookRotation(this.moveDirection, -this.gravityDirection), 0);
                            else
                                fakeItemBoxInstance = UnityEngine.Object.Instantiate(this.fakeItemBoxPrefab, fakeItemBoxPosition, Quaternion.LookRotation(this.moveDirection, -this.gravityDirection));
                            FakeItemBoxBehavior fakeItemBoxScript = ((GameObject)(fakeItemBoxInstance)).gameObject.GetComponent<FakeItemBoxBehavior>();
                            if (fakeItemBoxScript != null)
                                fakeItemBoxScript.playerIndex = this.playerIndex;
                        }
                        break;

                    case ItemType.Immunity:
                        this.spawnedTimer = 0.0f;
						this.SendMessage("OnShieldStarted");
                        if (this.invincibleTimer < this.immunityDuration)
                            this.invincibleTimer = this.immunityDuration;
                        break;

                    case ItemType.SideBlades:
                        if (this.sideBladeOne == null)
                        {
                            UnityEngine.Object newInstance = null;
                            if (Network.connections.Length > 0)
                                newInstance = UnityEngine.Network.Instantiate(this.sideBladePrefab, this.transform.position /* + (this.transform.right * 1.0f) + (this.transform.right * this.sideBladePrefab.transform.localScale.x * 0.5f) */, Quaternion.LookRotation(this.transform.forward, this.transform.up), 0);
                            else
                                newInstance = UnityEngine.Object.Instantiate(this.sideBladePrefab, this.transform.position /* + (this.transform.right * 1.0f) + (this.transform.right * this.sideBladePrefab.transform.localScale.x * 0.5f) */, Quaternion.LookRotation(this.transform.forward, this.transform.up));
                            this.sideBladeOne = (GameObject)(newInstance);
                            this.sideBladeOne.transform.parent = this.transform;

                            SideBladeBehavior sideBladeScript = this.sideBladeOne.GetComponent<SideBladeBehavior>();
                            if (sideBladeScript != null)
                                sideBladeScript.playerIndex = this.playerIndex;
                        }

                        //if (this.sideBladeTwo == null)
                        //{
                        //    UnityEngine.Object newInstance = null;
                        //    if (Network.connections.Length > 0)
                        //        newInstance = UnityEngine.Network.Instantiate(this.sideBladePrefab, this.transform.position - (this.transform.right * 1.0f) - (this.transform.right * this.sideBladePrefab.transform.localScale.x * 0.5f), Quaternion.LookRotation(this.transform.forward, -this.transform.up), 0);
                        //    else
                        //        newInstance = UnityEngine.Object.Instantiate(this.sideBladePrefab, this.transform.position - (this.transform.right * 1.0f) - (this.transform.right * this.sideBladePrefab.transform.localScale.x * 0.5f), Quaternion.LookRotation(this.transform.forward, -this.transform.up));
                        //    this.sideBladeTwo = (GameObject)(newInstance);
                        //    this.sideBladeTwo.transform.parent = this.transform;

                        //    SideBladeBehavior sideBladeScript = this.sideBladeTwo.GetComponent<SideBladeBehavior>();
                        //    if (sideBladeScript != null)
                        //        sideBladeScript.playerIndex = this.playerIndex;
                        //}

                        this.sideBladeTimer = this.sideBladeDuration;
						this.SendMessage("OnSawbladeStarted");
                        break;
                }

                this.heldItem = ItemType.None;
            }

            // Set driving speed
            if (this.boostTime > 0.0f)
            {
                this.updateColor = true;
                this.currentSpeed = this.boostSpeed;
                this.boostTime -= Time.deltaTime;
				if (this.boostTime <= 0.0f)
					this.ShoutMessage("OnBoostEnded");
            }
            else
            {
                this.updateColor = true;
                this.currentSpeed = this.baseSpeed;
            }

            RaycastHit raycastHit;
            // First check if player is in the air...
            if (!(Physics.Linecast(gravityRayStart, gravityRayEnd, out raycastHit, DriverController.drivableLayerMask /*| DriverController.nonDrivableLayerMask*/)))
            {
                // ...and if he is, check if there is a wall behind him
                if (Physics.Linecast(this.transform.position - (this.gravityDirection * (this.characterController.height / 2.0f)), this.transform.position - (this.moveDirection * (this.characterController.radius + 3.0f)), out raycastHit, DriverController.drivableLayerMask /*| DriverController.nonDrivableLayerMask*/))
                {
                    // Change direction if a wall was detected
                    Vector3 oldMoveDirection = this.moveDirection;
                    Vector3 oldGravityDirection = this.gravityDirection;

                    Vector3 temp = -this.moveDirection;
                    this.moveDirection = this.gravityDirection;
                    this.gravityDirection = temp;
                    this.characterController.Move(((this.characterController.radius + 0.1f) * this.moveDirection));

                    Vector3 cornerPos = Vector3.zero;                        
                    if (this.nodeList.Count > 0)
                        cornerPos = this.nodeList[this.nodeList.Count - 1].position + Vector3.Project(this.GetNextNode().position - this.nodeList[this.nodeList.Count - 1].position, oldMoveDirection);
                    this.nodeList.Add(new PathNode(cornerPos, (-oldGravityDirection + oldMoveDirection).normalized, oldMoveDirection, nodeColor));
                }

                // Apply gravity
                totalMovement += this.gravityDirection * this.currentSpeed;
            }
            else
            {
                // If player is not in the air, check if there is a wall ahead of him 
                if (Physics.Linecast(	this.transform.position, 
										this.transform.position + (this.moveDirection * (this.characterController.radius + .25f)), 
										out raycastHit, 
										DriverController.drivableLayerMask 
										/*| DriverController.nonDrivableLayerMask*/))
                {
                    {
                        Vector3 cornerPos = Vector3.zero;
                        if (this.nodeList.Count > 0)
                            cornerPos = this.nodeList[this.nodeList.Count - 1].position + Vector3.Project(raycastHit.point - this.nodeList[this.nodeList.Count - 1].position, this.moveDirection);
                        this.nodeList.Add(new PathNode(cornerPos, (-this.gravityDirection - this.moveDirection).normalized, -this.moveDirection, nodeColor));

                        Vector3 temp = this.moveDirection;
                        this.moveDirection = -this.gravityDirection;
                        this.gravityDirection = temp;
						
                    }
                }

                // Check, if player can turn
                PathNode nextNode = this.GetNextNode();
                nextNode.color = nodeColor;
                Vector3 nodeOnDriver = nextNode.position; // this.nextNode.position;
                Quaternion transformRotation = this.transform.rotation;
                this.transform.rotation = Quaternion.LookRotation(this.moveDirection, -this.gravityDirection);
                nodeOnDriver = this.transform.InverseTransformDirection(nodeOnDriver);
                nodeOnDriver.y = this.transform.InverseTransformDirection(this.transform.position).y;
                nodeOnDriver = this.transform.TransformDirection(nodeOnDriver);
                this.transform.rotation = transformRotation;

                float nodeDistance = (this.currentSpeed * this.moveDirection * Time.deltaTime).magnitude - (nodeOnDriver - this.transform.position).magnitude;

                if ((this.nodeList.Count <= 0 || nextNode.position != this.nodeList[this.nodeList.Count - 1].position) && nodeDistance >= 0 && (nextNode.position - this.transform.position).magnitude <= this.gridSize)
                {
                    switch (this.playerAction)
                    {
                        case PlayerAction.TurnLeft:
                            this.moveDirection = Vector3.Cross(this.gravityDirection, this.moveDirection);
                            this.transform.position = nodeOnDriver - ((nodeOnDriver - this.transform.position).magnitude * this.moveDirection);
                            this.nodeList.Add(nextNode);
                            this.SendMessage("OnPlayerTurned");
                            break;
                        case PlayerAction.TurnRight:
                            this.moveDirection = Vector3.Cross(this.moveDirection, this.gravityDirection);
                            this.transform.position = nodeOnDriver - ((nodeOnDriver - this.transform.position).magnitude * this.moveDirection);
                            this.nodeList.Add(nextNode);
                            this.SendMessage("OnPlayerTurned");
                            break;
                        default:
                            break;
                    }

                    this.playerAction = PlayerAction.None;
                    if (this.driverInput != null)
                        this.driverInput.playerAction = PlayerAction.None;
                }

                // Apply driving speed
                totalMovement += (this.currentSpeed * this.moveDirection);
            }

            // Move player
            totalMovement = new Vector3((float)(Math.Round(totalMovement.x, 1)), (float)(Math.Round(totalMovement.y, 1)), (float)(Math.Round(totalMovement.z, 1)));
            if (this.characterController.enabled)
                this.characterController.Move(totalMovement * Time.deltaTime);

            // Clean up node list
            this.NodeListCleanup(this.baseTrailLength, nodeColor);

            // Create new trail collision
            int currentCollider = 0;
            for (int i = 0; i < this.nodeList.Count - 1; i++)
            {
                Vector3 lookDirection = this.nodeList[i + 1].position - this.nodeList[i].position;
                if (currentCollider >= this.colliderList.Count)
                {
                    if (lookDirection.sqrMagnitude > 0)
                    {
                        UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.trailCollisionSegment, this.nodeList[i].position + (lookDirection / 2.0f) + (((PathNode)(this.nodeList[i])).nextNormal * (this.trailCollisionSegment.localScale.y / 2.0f)), Quaternion.LookRotation(lookDirection, this.nodeList[i].normal));
                        ((Transform)(newObject)).localScale = new Vector3(this.trailCollisionSegment.localScale.x, this.trailCollisionSegment.localScale.y, lookDirection.magnitude);
                        ((Transform)(newObject)).parent = this.trailCollisionObject.transform;
                        this.colliderList.Add((Transform)(newObject));
                        currentCollider++;
                    }
                }
                else
                {
                    if (this.colliderList.Count > i && this.colliderList[i] != null)
                    {
                        this.colliderList[i].collider.enabled = true;
                        this.colliderList[i].localScale = new Vector3(this.trailCollisionSegment.localScale.x, this.trailCollisionSegment.localScale.y, lookDirection.magnitude);
                        this.colliderList[i].position = this.nodeList[i].position + (lookDirection / 2.0f) + (((PathNode)(this.nodeList[i])).nextNormal * (this.trailCollisionSegment.localScale.y / 2.0f));
                        if (!Vector3.zero.Equals(lookDirection) && 
							0.0f != lookDirection.sqrMagnitude)
                            this.colliderList[i].rotation = Quaternion.LookRotation(lookDirection, this.nodeList[i].normal);
                        currentCollider++;
                    }
                }
            }

            for (int i = currentCollider; i < this.colliderList.Count && this.colliderList[i] != null; i++)
            {
                this.colliderList[i].collider.enabled = false;
            }

            if (this.transform.position.magnitude > this.arenaSettings.maxDistance)
                this.Kill(-2, this.playerIndex);
        }

        // Player transformations
		if (!Vector3.zero.Equals(this.moveDirection) 
				&& 0.0f != this.moveDirection.sqrMagnitude){
        	this.transform.rotation = Quaternion.Slerp(
					this.transform.rotation, 
					Quaternion.LookRotation(this.moveDirection, -this.gravityDirection), 
					this.rotationSpeed * Time.deltaTime);
		}

        if (this.cameraTransform != null)
        {
            Vector3 targetPos = this.cameraDistance.x * this.transform.right + this.cameraDistance.y * this.transform.up + this.cameraDistance.z * this.transform.forward;
            this.cameraPos = Vector3.RotateTowards(this.cameraPos, targetPos, 6.0f * Time.deltaTime, 5.0f * Time.deltaTime);
            this.cameraTransform.position = this.transform.position + this.cameraPos;

            targetPos = this.cameraAngleShift.x * this.transform.right + this.cameraAngleShift.y * this.transform.up + this.cameraAngleShift.z * this.transform.forward;
            this.cameraTransform.rotation = Quaternion.LookRotation(-this.cameraPos + targetPos, this.transform.up);
        }

        if (this.killed && (Network.connections.Length <= 0 || this.networkView.isMine))
        {
            this.killTimer -= Time.deltaTime;
            if (this.killTimer <= 0.0f)
            {
                if (Network.connections.Length > 0)
                    UnityEngine.Network.Destroy(this.gameObject);
                else
                    UnityEngine.Object.Destroy(this.gameObject);
            }
        }

        if (this.lineRenderer != null)
        {
            this.lineRenderer.pointList = this.nodeList;
            this.lineRenderer.baseLength = this.baseTrailLength;
        }

        if (this.killed)
        {
            this.killedTrailLength = Math.Max(0, this.killedTrailLength - Time.deltaTime * 30.0f);
            if (this.killedTrailLength <= 0.0f && this.lineRendererKilled != null)
                this.lineRendererKilled.pointList = null;

            if (this.nodeList.Count > 0)
                this.nodeList.RemoveAt(this.nodeList.Count - 1);

            this.insertedNode = false;
            this.removedNode = false;

            this.NodeListCleanup(this.killedTrailLength, nodeColor);
        }

        if (this.sideBladeOne != null)
        {
            SideBladeBehavior sideBladeScript = this.sideBladeOne.GetComponent<SideBladeBehavior>();
            if (sideBladeScript != null)
                sideBladeScript.moveDirection = totalMovement;
        }
        if (this.sideBladeTwo != null)
        {
            SideBladeBehavior sideBladeScript = this.sideBladeTwo.GetComponent<SideBladeBehavior>();
            if (sideBladeScript != null)
                sideBladeScript.moveDirection = totalMovement;
        }
        if (this.sideBladeTimer > 0.0f)
        {
            this.sideBladeTimer -= Time.deltaTime;
            if (this.sideBladeTimer <= 0.0f)
                this.KillSideBlades();
        }
    }


    // Clean up node list
    private void NodeListCleanup(float trailLength, Color nodeColor)
    {
        if (this.nodeList.Count <= 0)
            return;

        Vector3 groundPos = this.nodeList[this.nodeList.Count - 1].position + Vector3.Project(this.transform.position - this.nodeList[this.nodeList.Count - 1].position, this.moveDirection);
        this.nodeList.Add(new PathNode(groundPos, -this.gravityDirection, -this.gravityDirection, nodeColor));
        float currentLength = 0.0f;
        float totalLength = 0.0f;
        PathNode firstRemoved = (PathNode)(this.nodeList[0]);

        for (int i = this.nodeList.Count - 2; i >= 0; i--)
        {
            currentLength += (this.nodeList[i].position - this.nodeList[i + 1].position).magnitude;
            if (currentLength > trailLength * this.gridSize)
            {
                if (!(this.removedNode))
                {
                    firstRemoved = (PathNode)(this.nodeList[i]);
                }
                this.nodeList.RemoveAt(i);
                this.removedNode = true;
            }
            if (currentLength <= trailLength * this.gridSize)
                totalLength = currentLength;
        }

        if (!(this.killed))
            this.killedTrailLength = currentLength;

        Vector3 directionVector = (firstRemoved.position - this.nodeList[0].position).normalized;
        if (totalLength < trailLength * this.gridSize && currentLength > trailLength * this.gridSize)
        {
            float difference = (trailLength * this.gridSize) - totalLength;
            this.nodeList.Insert(0, new PathNode(this.nodeList[0].position + (difference * directionVector), firstRemoved.nextNormal, firstRemoved.nextNormal, nodeColor));
            this.insertedNode = true;
        }
    }


    // Select this driver's mesh
    public void SetMesh(int meshIndex)
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("SetMeshRPC", RPCMode.All, meshIndex);
        else
            this.SetMeshRPC(meshIndex);
    }


    [RPC]
    void SetMeshRPC(int meshIndex)
    {
        if (meshIndex >= 0 && meshIndex < this.meshList.GetLength(0))
            this.meshList[meshIndex].SetActive(true);
    }


    // Kill driver
    public void Kill(int killer, int killedPlayer)
    {
		Debug.Log("Kill: "+killer+" - "+killedPlayer);
        if (!(this.killed))
        {
            if (Network.connections.Length > 0)
                this.networkView.RPC("KillRPC", RPCMode.All, killer, killedPlayer);
            else
                this.KillRPC(killer, killedPlayer);

            if (this.ScoreFunction != null)
                this.ScoreFunction(killer, killedPlayer);
        }
    }


    [RPC]
    void KillRPC(int killer, int killedPlayer)
    {
        this.invincibleTimer = 0.0f;
        this.boostTime = 0.0f;

        if (this.killed)
            return;

        this.killed = true;

		this.SendMessage("OnPlayerDied");

        this.harmlessTimer = 100.0f;
        this.UpdateColors();

        if (this.lineRenderer != null)
            this.lineRenderer.gameObject.transform.parent = null;

        UnityEngine.Object explosion = UnityEngine.Object.Instantiate(this.explosionPrefab, this.transform.position, this.transform.rotation);
		foreach (string systemName in this.explosionParticlesNames){
	        ParticleSystem particleSystem = ((Transform)(explosion)).Find(systemName).gameObject.particleSystem;
	        particleSystem.startColor = 0.1f * particleSystem.startColor + 0.9f * this.mainColor;
	        particleSystem.startColor = new Color(particleSystem.startColor.r, particleSystem.startColor.g, particleSystem.startColor.b, 1.0f);
		}
        //ParticleSystem particleSystem = ((Transform)(explosion)).gameObject.GetComponent<ParticleSystem>();
        //ParticleSystem particleSystem = this.explosionPrefab.Find(this.explosionParticlesName).gameObject.particleSystem;
        //particleSystem.startColor = 0.1f * particleSystem.startColor + 0.9f * this.mainColor;
        //particleSystem.startColor = new Color(particleSystem.startColor.r, particleSystem.startColor.g, particleSystem.startColor.b, 1.0f);
        //particleSystem = particleSystem.gameObject.GetComponentInChildren<ParticleSystem>();
        //particleSystem.startColor = 0.1f * particleSystem.startColor + 0.9f * this.mainColor;
        //particleSystem.startColor = new Color(particleSystem.startColor.r, particleSystem.startColor.g, particleSystem.startColor.b, 1.0f);
        UnityEngine.Object.Destroy(((Transform)(explosion)).gameObject, 3.0f);

        if (this.lineRenderer != null)
        {
            UnityEngine.Object.Destroy(this.lineRenderer.gameObject, this.killTimer);
            this.lineRendererKilled = this.lineRenderer;
            this.lineRenderer = null;
        }

        if (this.vehicleMesh != null)
        {
            UnityEngine.Object.Destroy(this.vehicleMesh);
            this.vehicleMesh = null;
        }

        for (int i = 0; i < this.colliderList.Count; i++)
        {
            if (this.colliderList[i] != null)
            {
                this.colliderList[i].collider.enabled = false;
                this.colliderList[i] = null;
            }
        }
        if (this.trailCollisionObject != null)
        {
//#if UNITY_EDITOR            
//            UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Pause");
//            UnityEngine.Object.Destroy(this.trailCollisionObject, 1.0f);
//#else
//            UnityEngine.Object.Destroy(this.trailCollisionObject);
//#endif
            UnityEngine.Object.Destroy(this.trailCollisionObject);
            this.trailCollisionObject = null;
        }

        if (this.characterController != null)
            this.characterController.enabled = false;

        this.KillSideBlades();
    }

    void KillSideBlades()
    {
        if (!((Network.connections.Length <= 0 || this.networkView.isMine)))
            return;

        if (this.sideBladeOne != null)
        {
            if (Network.connections.Length > 0)
                UnityEngine.Network.Destroy(this.sideBladeOne);
            else
                UnityEngine.Object.Destroy(this.sideBladeOne);
            this.sideBladeOne = null;
        }

        if (this.sideBladeTwo != null)
        {
            if (Network.connections.Length > 0)
                UnityEngine.Network.Destroy(this.sideBladeTwo);
            else
                UnityEngine.Object.Destroy(this.sideBladeTwo);
            this.sideBladeTwo = null;
        }

        this.sideBladeTimer = 0.0f;
    }


    void GetRandomItem(ItemBoxBehavior itemBoxScript)
    {
        if (this.heldItem == ItemType.None)
        {
            itemBoxScript.SetInactive();
            int randomValue = UnityEngine.Random.Range(0, 4);

            switch (randomValue)
            {
                case 0:
                    this.heldItem = ItemType.Boost;
                    break;

                case 1:
                    this.heldItem = ItemType.FakeItemBox;
                    break;

                case 2:
                    this.heldItem = ItemType.SideBlades;
                    break;

                case 3:
                    this.heldItem = ItemType.Immunity;
                    break;
            }
        }
    }
    
    
    
    // On trigger enter
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == this.trailCollisionSegment.tag || collider.tag == this.tag)
        {
            if (!((Network.connections.Length <= 0 || this.networkView.isMine)) || this.invincibleTimer > 0.0f || this.killed)
                return;

            if (collider.gameObject.transform.parent == this.trailCollisionObject.transform)
            {
                int lastEnabled = 0;
                for (int i = 0; i < this.colliderList.Count; i++)
                {
                    if (this.colliderList[i].collider.enabled)
                        lastEnabled = i;
                }

                for (int i = Math.Max(0, lastEnabled - 2); i <= lastEnabled; i++)
                {
                    if (collider.transform == this.colliderList[i].transform)
                        return;
                }
            }

            DriverController otherDriver = null;
            if (collider.tag == this.tag)
                otherDriver = collider.gameObject.GetComponent<DriverController>();
            else if (collider.tag == this.trailCollisionSegment.tag)
                otherDriver = collider.gameObject.transform.parent.GetComponent<TrailCollision>().owner;

            int killer = -1;
            if (otherDriver != null)
                killer = otherDriver.playerIndex;

            this.Kill(killer, this.playerIndex);

            // Check if driver is crashing into other driver and kill both
            if (otherDriver != null)
            {
                if (otherDriver.invincibleTimer <= 0.0f && (otherDriver.transform.position - this.transform.position).magnitude <= 0.9f)
                {
                    otherDriver.Kill(this.playerIndex, this.playerIndex);
                }
            }

        }

        if (collider.tag == "NonDrivable" && collider.transform.parent != this)
        {
            if (!((Network.connections.Length <= 0 || this.networkView.isMine)) || this.killed)
                return;

            this.Kill(-1, this.playerIndex);
        }

        if (collider.tag == this.itemBoxPrefab.tag)
        {
            if (Network.connections.Length <= 0 || this.networkView.isMine)
                this.GetRandomItem(collider.gameObject.GetComponent<ItemBoxBehavior>());
        }

        if (collider.tag == this.fakeItemBoxPrefab.tag)
        {
            if (!((Network.connections.Length <= 0 || this.networkView.isMine)) || this.invincibleTimer > 0.0f || this.killed)
                return;

            FakeItemBoxBehavior fakeItemBoxScript = collider.gameObject.GetComponent<FakeItemBoxBehavior>();
            if (fakeItemBoxScript != null)
                this.Kill(fakeItemBoxScript.playerIndex, this.playerIndex);
            else
                this.Kill(-1, this.playerIndex);

            if (Network.connections.Length > 0)
                UnityEngine.Network.Destroy(collider.gameObject);
            else
                UnityEngine.Object.Destroy(collider.gameObject);
        }

        if (collider.tag == this.sideBladePrefab.tag)
        {
            if (!((Network.connections.Length <= 0 || this.networkView.isMine)) || this.invincibleTimer > 0.0f || this.killed)
                return;

            if (collider.transform.parent != this.transform)
            {
                SideBladeBehavior sideBladeScript = collider.gameObject.GetComponent<SideBladeBehavior>();
                if (sideBladeScript != null)
                    this.Kill(sideBladeScript.playerIndex, this.playerIndex);
                else
                    this.Kill(-1, this.playerIndex);
            }
        }
    }

    // On trigger stay
    void OnTriggerStay(Collider collider)
    {
        this.OnTriggerEnter(collider);
    }

    // Send data over network
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        float colorR = 0.0f;
        float colorG = 0.0f;
        float colorB = 0.0f;
        float colorA = 0.0f;
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        int totalNodes = 0;
        Vector3 nodePos = Vector3.zero;
        Vector3 nodeNormal = Vector3.zero;
        Vector3 nodeNextNormal = Vector3.zero;

        if (stream.isWriting)
        {
            // Sending data...
            stream.Serialize(ref this.gameStarted);
            stream.Serialize(ref this.playerIndex);
            stream.Serialize(ref this.boostTime);
            stream.Serialize(ref this.currentSpeed);
            colorR = this.mainColor.r;
            colorG = this.mainColor.g;
            colorB = this.mainColor.b;
            colorA = this.mainColor.a;
            stream.Serialize(ref colorR);
            stream.Serialize(ref colorG);
            stream.Serialize(ref colorB);
            stream.Serialize(ref colorA);
            stream.Serialize(ref this.moveDirection);
            stream.Serialize(ref this.gravityDirection);
            position = this.transform.position;
            stream.Serialize(ref position);
            rotation = this.transform.rotation;
            stream.Serialize(ref rotation);
            stream.Serialize(ref this.killed);
            stream.Serialize(ref this.invincibleTimer);

            totalNodes = this.nodeList.Count;
            stream.Serialize(ref totalNodes);
            for (int i = 0; i < this.nodeList.Count; i++)
            {
                nodePos = this.nodeList[i].position;
                stream.Serialize(ref nodePos);
                nodeNormal = this.nodeList[i].normal;
                stream.Serialize(ref nodeNormal);
                nodeNextNormal = ((PathNode)(this.nodeList[i])).nextNormal;
                stream.Serialize(ref nodeNextNormal);
            }
        }
        else
        {
            // Receiving data...
            stream.Serialize(ref this.gameStarted);
            stream.Serialize(ref this.playerIndex);
            stream.Serialize(ref this.boostTime);
            stream.Serialize(ref this.currentSpeed);
            stream.Serialize(ref colorR);
            stream.Serialize(ref colorG);
            stream.Serialize(ref colorB);
            stream.Serialize(ref colorA);
            Color currentColor = new Color(colorR, colorG, colorB, colorA);
            if (!(this.mainColor.Equals(currentColor)))
            {
                this.mainColor = currentColor;
                this.updateColor = true;
            }
            stream.Serialize(ref this.moveDirection);
            stream.Serialize(ref this.gravityDirection);
            stream.Serialize(ref position);
            this.transform.position = position;
            stream.Serialize(ref rotation);
            this.transform.rotation = rotation;
            stream.Serialize(ref this.killed);
            stream.Serialize(ref this.invincibleTimer);

            stream.Serialize(ref totalNodes);
            for (int i = 0; i < totalNodes; i++)
            {
                stream.Serialize(ref nodePos);
                stream.Serialize(ref nodeNormal);
                stream.Serialize(ref nodeNextNormal);
                if (i < this.nodeList.Count)
                    this.nodeList[i] = new PathNode(nodePos, nodeNormal, nodeNextNormal, this.mainColor);
                else
                    this.nodeList.Add(new PathNode(nodePos, nodeNormal, nodeNextNormal, this.mainColor));
            }
            while (this.nodeList.Count > totalNodes)
                this.nodeList.RemoveAt(this.nodeList.Count - 1);
        }
    }

    // Call when instantiated on network
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (!(this.networkView.isMine))
        {
            this.arenaSettings = GameObject.Find("Arena").GetComponent<ArenaSettings>();
            GameState gameState = GameObject.FindWithTag("GameState").GetComponent<GameState>();
            this.playerIndex = gameState.playerIndex;
        }
    }
}

public delegate void ScoreDelegate(int killer, int killedPlayer);


public enum ItemType
{
    None,
    Boost,
    FakeItemBox,
    SideBlades,
    Immunity,
    InvertControls
}
