using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Animation))]
public class DriverController : MonoBehaviour
{
    public AnimationClip drivingAnimation;
    public float baseSpeed = 20.0f;
    public Transform cameraTransform = null;

    public GameObject debugGui = null;

    private Animation driverAnimation;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 sideVector = Vector3.zero;
    private Vector3 cameraPos = Vector3.zero;
    private GameObject lastNode = null;
    private Vector3 nodeContact = Vector3.zero;
    private Vector3 frontPath = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 leftPath = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 rightPath = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 nodeNormal = new Vector3(0.0f, 1.0f, 1.0f);
    private Vector3 pathLocation = Vector3.zero;
    private Vector3 individualGravity = new Vector3(0.0f, -1.0f, 0.0f);
    private bool canMove = false;


    // Use this for initialization
    void Awake()
    {
        this.driverAnimation = (Animation)(GetComponent(typeof(Animation)));
        if (!(this.driverAnimation))
            Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");

        if (!(this.drivingAnimation))
        {
            this.driverAnimation = null;
            Debug.Log("No driving animation found. Turning off animations.");
        }

        this.moveDirection = this.transform.forward;
        this.sideVector = this.transform.right;
        this.moveDirection.Normalize();
        this.sideVector.Normalize();
        this.cameraPos = (-(this.individualGravity) * 10) + (this.moveDirection * -20);

        this.rigidbody.velocity = individualGravity * Physics.gravity.magnitude;
    }


    // Update is called once per frame
    void Update()
    {
        // Move/control player
        if (canMove && (this.transform.position - this.nodeContact).sqrMagnitude > (this.lastNode.gameObject.transform.position - this.nodeContact).sqrMagnitude)
        {
            float difference = (this.transform.position - this.lastNode.transform.position).magnitude;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                float colliderHeight = this.GetComponent<SphereCollider>().radius;
                this.transform.position = this.lastNode.transform.position + (nodeNormal * colliderHeight);
                this.moveDirection = this.leftPath.normalized;
                this.transform.position += this.moveDirection * difference;
                this.individualGravity = -(this.nodeNormal);
                this.sideVector = Vector3.Cross(-(this.individualGravity), this.moveDirection).normalized;

                this.rigidbody.velocity = this.moveDirection;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                float colliderHeight = this.GetComponent<SphereCollider>().radius;
                this.transform.position = this.lastNode.transform.position + (nodeNormal * colliderHeight);
                this.moveDirection = this.rightPath.normalized;
                this.transform.position += this.moveDirection * difference;
                this.individualGravity = -(this.nodeNormal);
                this.sideVector = Vector3.Cross(-(this.individualGravity), this.moveDirection).normalized;

                this.rigidbody.velocity = this.moveDirection;
            }
            else
            {
                float colliderHeight = this.GetComponent<SphereCollider>().radius;
                this.transform.position = this.lastNode.transform.position + (nodeNormal * colliderHeight);
                this.moveDirection = this.frontPath.normalized;
                this.transform.position += this.moveDirection * difference;
                this.individualGravity = -(this.nodeNormal);
                this.sideVector = Vector3.Cross(-(this.individualGravity), this.moveDirection).normalized;

                this.rigidbody.velocity = this.moveDirection;
            }

            canMove = false;
        }

        Vector3 totalMovement = moveDirection * this.baseSpeed;
        Vector3 totalVelocity = totalMovement + (individualGravity * Physics.gravity.magnitude);

        this.rigidbody.velocity = new Vector3(
            Mathf.Clamp(this.rigidbody.velocity.x, Mathf.Min(totalVelocity.x, 0), Mathf.Max(totalVelocity.x, 0)),
            Mathf.Clamp(this.rigidbody.velocity.y, Mathf.Min(totalVelocity.y, 0), Mathf.Max(totalVelocity.y, 0)),
            Mathf.Clamp(this.rigidbody.velocity.z, Mathf.Min(totalVelocity.z, 0), Mathf.Max(totalVelocity.z, 0)));

        this.rigidbody.AddForce(totalMovement, ForceMode.VelocityChange);
        this.rigidbody.AddForce(individualGravity * Physics.gravity.magnitude, ForceMode.Force);

        this.transform.rotation = Quaternion.LookRotation(this.moveDirection, -(this.individualGravity));

        // Camera
        if (this.cameraTransform != null)
        {
            Vector3 targetDir = (-(this.individualGravity) * 10) + (this.moveDirection * -20);
            this.cameraPos = Vector3.RotateTowards(this.cameraPos, targetDir, 0.1f, 10.0f);
            this.cameraTransform.position = this.transform.position + this.cameraPos;
            this.cameraTransform.rotation = Quaternion.LookRotation((-(this.cameraPos)).normalized, -(this.individualGravity).normalized);
        }


        // Animations
        if (this.driverAnimation)
        {
            this.driverAnimation[this.drivingAnimation.name].speed = 1.0f;
            this.driverAnimation[this.drivingAnimation.name].wrapMode = WrapMode.Loop;
            this.driverAnimation.CrossFade(this.drivingAnimation.name);
        }

        // Display debug information
        if (this.debugGui != null)
        {
            this.debugGui.guiText.text += "Player move direction: X=" + this.moveDirection.x + ", Y=" + this.moveDirection.y + ", Z=" + this.moveDirection.z + "\n";
            this.debugGui.guiText.text += "Player side vector: X=" + this.sideVector.x + ", Y=" + this.sideVector.y + ", Z=" + this.sideVector.z + "\n";
            Vector3 totalGravity = individualGravity * Physics.gravity.magnitude;
            this.debugGui.guiText.text += "Player gravity: X=" + totalGravity.x + ", Y=" + totalGravity.y + ", Z=" + totalGravity.z + "\n";
            this.debugGui.guiText.text += "Total player velocity: X=" + totalVelocity.x + ", Y=" + totalVelocity.y + ", Z=" + totalVelocity.z + "\n";
            this.debugGui.guiText.text += "\n";
            this.debugGui.guiText.text += "Player rigid body velocity: X=" + this.rigidbody.velocity.x + ", Y=" + this.rigidbody.velocity.y + ", Z=" + this.rigidbody.velocity.z + "\n";
            this.debugGui.guiText.text += "Player forward: X=" + this.transform.up.x + ", Y=" + this.transform.up.y + ", Z=" + this.transform.up.z + "\n";
            this.debugGui.guiText.text += "Player right: X=" + this.transform.right.x + ", Y=" + this.transform.right.y + ", Z=" + this.transform.right.z + "\n";
        }
    }


    // Trigger collision
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("PathNode") && lastNode != collider.gameObject)
        {
            this.lastNode = collider.gameObject;
            this.nodeContact = this.transform.position;
            PathNode pathNode = collider.gameObject.GetComponent<PathNode>();
            canMove = true;
            this.nodeNormal = pathNode.upVector.normalized;

            List<float> angles = new List<float>(pathNode.directions.Count);

            float forward = -1.0f;
            int forwardIndex = -1;

            float left = 1.0f;
            int leftIndex = -1;
            float right = 1.0f;
            int rightIndex = -1;

            for (int i = 0; i < pathNode.directions.Count; i++)
            {
                angles.Add(Vector3.Dot(this.moveDirection, pathNode.directions[i]));
                if (angles[i] >= forward)
                {
                    forwardIndex = i;
                    forward = angles[i];
                }

                if (forwardIndex < 0)
                    forwardIndex = 0;
                this.frontPath = pathNode.directions[forwardIndex];


                Vector3 crossProduct = Vector3.zero;
                float dotProduct = 0.0f;

                if ((angles[i] <= left || angles[i] <= right) && (angles[i] >= -0.9f))
                {
                    crossProduct = Vector3.Cross(pathNode.directions[i], this.frontPath);
                    dotProduct = Vector3.Dot(pathNode.upVector, crossProduct);

                    if (dotProduct <= 0.0f)
                    {
                        if (angles[i] <= right)
                        {
                            rightIndex = i;
                            right = angles[i];
                        }
                    }
                    else
                    {
                        if (angles[i] <= left)
                        {
                            leftIndex = i;
                            left = angles[i];
                        }
                    }                    
                }

                if (leftIndex < 0)
                    leftIndex = forwardIndex;
                if (rightIndex < 0)
                    rightIndex = forwardIndex;
                this.leftPath = pathNode.directions[leftIndex];
                this.rightPath = pathNode.directions[rightIndex];
            }
        }
    }

    // Collision with collider
    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            SphereCollider bikeHitbox = this.GetComponent<SphereCollider>();
            float colliderHeight = bikeHitbox.radius;

            foreach (ContactPoint contact in collision.contacts)
            {
                float dotAngle = Vector3.Dot(this.transform.up, contact.normal);

                if (contact.thisCollider == bikeHitbox /* && dotAngle > 0.001f && dotAngle < 0.999f */)
                {
                    this.individualGravity = -(contact.normal);
                    this.moveDirection = Vector3.Cross(this.sideVector, contact.normal).normalized;
                    this.sideVector = Vector3.Cross(contact.normal, this.moveDirection).normalized;

                    this.individualGravity = new Vector3((float)(Math.Round(this.individualGravity.x, 3)), (float)(Math.Round(this.individualGravity.y, 3)), (float)(Math.Round(this.individualGravity.z, 3)));
                    this.moveDirection = new Vector3((float)(Math.Round(this.moveDirection.x, 3)), (float)(Math.Round(this.moveDirection.y, 3)), (float)(Math.Round(this.moveDirection.z, 3)));
                    this.sideVector = new Vector3((float)(Math.Round(this.sideVector.x, 3)), (float)(Math.Round(this.sideVector.y, 3)), (float)(Math.Round(this.sideVector.z, 3)));

                    //this.transform.position = contact.point + (contact.normal * colliderHeight);
                    //this.rigidbody.velocity = this.moveDirection + this.individualGravity;
                }
            }
        }
    }


    // Reset GameObject
    void Reset()
    {
        gameObject.tag = "Player";
    }
}
