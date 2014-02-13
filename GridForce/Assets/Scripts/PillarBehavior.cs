using UnityEngine;
using System.Collections;

public class PillarBehavior : MonoBehaviour
{
    public ItemBoxBugFix itemBoxBugFix = null;

    private bool networkInstance = false;

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        this.networkInstance = true;

        ArenaSettings arenaSettings = GameObject.FindObjectOfType<ArenaSettings>();
        if (arenaSettings != null)
            ArenaSettings.SetTextureScales(this.gameObject, arenaSettings.gridSize);
    }

    // Reinstantiate object on level load
    public void ReInstantiate()
    {
        if (Network.connections.Length > 0)
        {
            GameObject networkInstance = (GameObject)(Network.Instantiate(itemBoxBugFix.itemBoxPrefab, this.transform.position, this.transform.rotation, 0));
            Travelling travelComponent = this.GetComponent<Travelling>();
            Travelling networkTravelComponent = networkInstance.GetComponent<Travelling>();
            networkTravelComponent.pathNodes = travelComponent.pathNodes;
            networkTravelComponent.speed = travelComponent.speed;
            networkTravelComponent.sloppyness = travelComponent.sloppyness;
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
    void KillAllLocalRPC()
    {
        GameObject[] pillarObjects = GameObject.FindGameObjectsWithTag("Pillar");
        foreach (GameObject pillar in pillarObjects)
        {
            PillarBehavior pillarScript = pillar.GetComponent<PillarBehavior>();
            if (pillarScript != null)
                pillarScript.KillIfLocal();
        }
    }

    public void KillAllLocal()
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("KillAllLocalRPC", RPCMode.All);
        else
            this.KillAllLocalRPC();
    }
}
