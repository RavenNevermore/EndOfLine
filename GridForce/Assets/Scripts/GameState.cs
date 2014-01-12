using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public ArenaSettings arenaSettings = null;    // Arena settings
    public Transform driverGameObject = null;     // Driver game object
    private bool gameStarted = false;

    private Transform currentPlayerObject = null;   // Current player object

    private static Color[] colorList = new Color[]   // List of possible colors
    {
        new Color(0.8f, 0.0f, 0.0f, 1.0f),
        new Color(0.0f, 0.8f, 0.0f, 1.0f),
        new Color(0.0f, 0.0f, 0.8f, 1.0f),
        new Color(0.6f, 0.0f, 0.6f, 1.0f)
    };

    private int spawnPoint = 0;    // This player's initial spawn point
    private Color playerColor = colorList[0];  // This player's colors

    // Use this for initialization
    void Start()
    {
        this.arenaSettings = GameObject.Find("Arena").GetComponent<ArenaSettings>();
        this.networkView.group = 0;
	}

    // Update is called once per frame
    void Update()
    {
        if (this.gameStarted && this.currentPlayerObject == null)
        {
            this.createDriverObject(Random.Range(0, this.arenaSettings.spawnPoints.Count - 1), this.playerColor);
        }
    }


    // Instantiate a player object
	void createDriverObject(int spawnpoint, Color playerColor)
    {
        int i = Mathf.Min(spawnpoint, this.arenaSettings.spawnPoints.Count - 1);
        UnityEngine.Object newObject = UnityEngine.Network.Instantiate(this.driverGameObject, this.arenaSettings.spawnPoints[i].position, this.arenaSettings.spawnPoints[i].rotation, 0);
		DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
        driver.arenaSettings = this.arenaSettings;
        driver.cameraTransform = this.arenaSettings.cameraTransform;
        this.currentPlayerObject = ((Transform)(newObject));
    }


    public IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.1f);

        int i = 0;
        foreach (NetworkPlayer player in Network.connections)
        {
            i++;
            this.networkView.RPC("AssignVariables", player, Mathf.Min(i, this.arenaSettings.spawnPoints.Count - 1), colorList[i].r, colorList[i].g, colorList[i].b, colorList[i].a);
        }

        this.networkView.RPC("StartGameRPC", RPCMode.All);
    }

    [RPC]
    void AssignVariables(int spawnPoint, float colorR, float colorG, float colorB, float colorA)
    {
        this.spawnPoint = spawnPoint;
        this.playerColor = new Color(colorR, colorG, colorB, colorA);
    }

    [RPC]
    void StartGameRPC()
    {
        GameObject.Find("preview_state").SetActive(false);
        this.arenaSettings.cameraTransform.gameObject.SetActive(true);

        this.gameStarted = true;
        this.createDriverObject(this.spawnPoint, this.playerColor);
    }


    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            StartCoroutine(this.StartGame());
    }
}
