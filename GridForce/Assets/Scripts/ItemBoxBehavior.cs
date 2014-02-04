using UnityEngine;
using System.Collections;

public class ItemBoxBehavior : MonoBehaviour
{
    public GameObject itemBoxMesh = null;
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

	private void playSound(string childName){
		Transform child = this.transform.FindChild(childName);
		if (null == child)
			return;
		AudioSource audio =  child.GetComponent<AudioSource>();
		if (null != audio){
			audio.Play();
		}
	}

    // Deactivate item box
    public void SetInactive()
    {
		this.playSound("Collected");
        if (Network.connections.Length > 0)
            this.networkView.RPC("SetInactiveRPC", RPCMode.All);
        else
            this.SetInactiveRPC();
    }

    [RPC]
    private void SetInactiveRPC()
    {
        this.timeUntilRespawn = this.respawnTime;

        this.itemBoxMesh.SetActive(false);
        this.itemBoxCollider.enabled = false;
    }

    // Activate item box
    public void SetActive()
    {
		this.playSound("Respawn");
        this.itemBoxMesh.SetActive(true);
        this.itemBoxCollider.enabled = true;
    }
}
