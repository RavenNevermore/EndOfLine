using UnityEngine;
using System.Collections;

public class Travelling : MonoBehaviour {

	public Transform[] pathNodes;
	public float speed;
	public float sloppyness;

	private int nextNodeIndex;
    private Vector3 travel = Vector3.zero;

	void Start ()
    {
		this.nextNodeIndex = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (null == this.pathNodes)
			return;
		
        if (this.pathNodes.GetLength(0) <= 0)
        {
            this.transform.Translate(this.travel);
            return;
        }

		Transform node = this.pathNodes[this.nextNodeIndex];

		Vector3 direction = node.position - this.transform.position;
        this.travel = direction.normalized * speed * Time.deltaTime;

        this.transform.Translate(this.travel);

		float distance = Vector3.Distance(node.position, 
		                                  this.transform.position);

		//Debug.Log("--> " + distance);
		if (this.sloppyness > distance)
        {
			this.nextNodeIndex = (this.nextNodeIndex + 1) % this.pathNodes.Length;
		}
	}


    // Send data over network
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 position = Vector3.zero;
        if (stream.isWriting)
        {
            // Sending data...
            position = this.transform.position;
            stream.Serialize(ref position);
            stream.Serialize(ref this.travel);
        }
        else
        {
            // Receiving data...
            stream.Serialize(ref position);
            this.transform.position = position;
            stream.Serialize(ref this.travel);
        }
    }
}
