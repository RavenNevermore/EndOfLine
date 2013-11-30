using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class DriverController : MonoBehaviour
{
    private CharacterController characterController;        // Character controller
    private Vector3 moveDirection;          // Character move direction
    private Vector3 gravityDirection;       // Character gravity

	// Use this for initialization
	void Start ()
    {
        this.characterController = this.gameObject.GetComponent<CharacterController>();
        this.moveDirection = this.transform.forward;
        this.gravityDirection = -this.transform.up;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!(this.characterController.isGrounded))
        {
            this.characterController.Move(this.gravityDirection * Physics.gravity.magnitude * Time.deltaTime);
        }

	}
}
