using UnityEngine;
using System.Collections;

public class ItemBoxBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer = null;
    public Collider itemBoxCollider = null;
    public float respawnTime = 10.0f;
    public ItemBoxBugFix itemBoxBugFix = null;

    private float timeUntilRespawn = 0.0f;

    private bool networkInstance = false;

    void Start()
    {
        if (!(this.networkInstance))
        {
            if (Network.isClient)
            {
                UnityEngine.Object.Destroy(this.gameObject);
            }
            else
            {
                this.SetInactiveRPC();
                this.timeUntilRespawn = 0.0f;
            }
        }
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        this.networkInstance = true;
    }

    // Reinstantiate object on level load
    public void ReInstantiate()
    {
        if (Network.connections.Length > 0)
        {
            Debug.Log("Instantiating over network item box...");
            Network.Instantiate(itemBoxBugFix.itemBoxPrefab, this.transform.position, this.transform.rotation, 0);
            UnityEngine.Object.Destroy(this.gameObject);
        }
        else
        {
            this.SetActive();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (this.timeUntilRespawn > 0.0f)
        {
            this.timeUntilRespawn -= Time.deltaTime;
            if (this.timeUntilRespawn <= 0.0f)
                this.SetActive();
        }	
	}

    // Deactivate item box
    public void SetInactive()
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("SetInactiveRPC", RPCMode.All);
        else
            this.SetInactiveRPC();
    }

    [RPC]
    private void SetInactiveRPC()
    {
        this.timeUntilRespawn = this.respawnTime;

        this.meshRenderer.enabled = false;
        this.itemBoxCollider.enabled = false;
    }

    // Activate item box
    public void SetActive()
    {
        this.meshRenderer.enabled = true;
        this.itemBoxCollider.enabled = true;
    }
}
