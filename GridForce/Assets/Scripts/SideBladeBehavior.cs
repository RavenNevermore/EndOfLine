using UnityEngine;
using System.Collections;

public class SideBladeBehavior : MonoBehaviour
{
    public int playerIndex = 0;   // Player index of driver who spawned fake item box
    public Vector3 moveDirection = Vector3.zero;
	
	// Update is called once per frame
	void Update()
    {
        if (Network.connections.Length > 0 && !(this.networkView.isMine))
            this.transform.position += this.moveDirection * Time.deltaTime;
	}


    // Send data over network
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 position = Vector3.zero;
        Vector3 forwardVector = Vector3.zero;
        Vector3 upVector = Vector3.zero;
        if (stream.isWriting)
        {
            // Sending data...
            position = this.transform.position;
            forwardVector = this.transform.forward;
            upVector = this.transform.up;
            stream.Serialize(ref position);
            stream.Serialize(ref forwardVector);
            stream.Serialize(ref upVector);
            stream.Serialize(ref moveDirection);
            stream.Serialize(ref this.playerIndex);
        }
        else
        {
            // Receiving data...
            stream.Serialize(ref position);
            stream.Serialize(ref forwardVector);
            stream.Serialize(ref upVector);
            this.transform.position = position;
            this.transform.rotation = Quaternion.LookRotation(forwardVector, upVector);
            stream.Serialize(ref moveDirection);
            stream.Serialize(ref this.playerIndex);
        }
    }
}
