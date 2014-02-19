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

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        this.networkInstance = true;
    }

    // Reinstantiate object on level load
    public void ReInstantiate()
    {
        if (Network.connections.Length > 0)
        {
            GameObject networkInstance = (GameObject)(Network.Instantiate(itemBoxBugFix.itemBoxPrefab, this.transform.position, this.transform.rotation, 0));
            Travelling travelComponent = this.GetComponent<Travelling>();
            Travelling networkTravelComponent = networkInstance.GetComponent<Travelling>();
            if (travelComponent != null && networkTravelComponent != null)
            {
                networkTravelComponent.pathNodes = travelComponent.pathNodes;
                networkTravelComponent.speed = travelComponent.speed;
                networkTravelComponent.sloppyness = travelComponent.sloppyness;
            }
        }
    }

    void KillIfLocal()
    {
        if (!(this.networkInstance) && Network.connections.Length > 0)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }
    }

    [RPC]
    void KillAllLocalItemboxRPC()
    {
        GameObject[] itemBoxes = GameObject.FindGameObjectsWithTag("ItemBox");
        foreach (GameObject itemBox in itemBoxes)
        {
            ItemBoxBehavior itemBoxScript = itemBox.GetComponent<ItemBoxBehavior>();
            if (itemBoxScript != null)
                itemBoxScript.KillIfLocal();
        }
    }

    public void KillAllLocal()
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("KillAllLocalItemboxRPC", RPCMode.All);
        else
            this.KillAllLocalItemboxRPC();
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

    // Copy a component to another GameObject
    Component CopyComponent(Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
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
