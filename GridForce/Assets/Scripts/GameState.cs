using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class GameState : MonoBehaviour
{
    public ArenaSettings arenaSettings = null;    // Arena settings
    public Transform driverGameObject = null;     // Driver game object
    private bool gameStarted = false;

    private Transform currentPlayerObject = null;   // Current player object

    public static Color[] colorArray = new Color[]    // List of possible colors
    {
        new Color(0.8f, 0.0f, 0.0f, 1.0f),
        new Color(0.0f, 0.0f, 0.8f, 1.0f),
        new Color(0.6f, 0.0f, 0.6f, 1.0f),
        new Color(0.0f, 0.8f, 0.0f, 1.0f)
    };
    private List<Color> colorList = new List<Color>();   // Randomized color list
    private List<int> spawnPointList = new List<int>();  // List of randomized points

    private int spawnPoint = 0;    // This player's initial spawn point
    private Color playerColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);  // This player's color

    // Use this for initialization
    void Start()
    {
        this.arenaSettings = GameObject.Find("Arena").GetComponent<ArenaSettings>();
        this.networkView.group = 0;

        for (int i = 0; i < this.arenaSettings.spawnPoints.Count; i++)
            this.spawnPointList.Add(i);
        for (int i = 0; i < colorArray.GetLength(0); i++)
            this.colorList.Add(colorArray[i]);

        colorList.Shuffle();
        spawnPointList.Shuffle();

        if (Network.connections.Length <= 0)
        {
            this.AssignVariables(Mathf.Min(this.spawnPointList[0], this.arenaSettings.spawnPoints.Count - 1), colorList[0].r, colorList[0].g, colorList[0].b, colorList[0].a);
            this.StartGameRPC();
        }
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
        UnityEngine.Object newObject = null;
        if (Network.connections.Length > 0)
            newObject = UnityEngine.Network.Instantiate(this.driverGameObject, this.arenaSettings.spawnPoints[i].position, this.arenaSettings.spawnPoints[i].rotation, 0);
        else
            newObject = UnityEngine.Object.Instantiate(this.driverGameObject, this.arenaSettings.spawnPoints[i].position, this.arenaSettings.spawnPoints[i].rotation);
		DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
        driver.arenaSettings = this.arenaSettings;
        driver.cameraTransform = this.arenaSettings.cameraTransform;
        driver.mainColor = this.playerColor;
        driver.updateColor = true;
        this.currentPlayerObject = ((Transform)(newObject));
    }

    // Start hosted game
    public IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.1f);

        this.AssignVariables(Mathf.Min(this.spawnPointList[0], this.arenaSettings.spawnPoints.Count - 1), colorList[0].r, colorList[0].g, colorList[0].b, colorList[0].a);

        int i = 0;        
        foreach (NetworkPlayer player in Network.connections)
        {
            i++;
            this.networkView.RPC("AssignVariables", player, Mathf.Min(this.spawnPointList[i], this.arenaSettings.spawnPoints.Count - 1), colorList[i].r, colorList[i].g, colorList[i].b, colorList[i].a);
        }

        this.networkView.RPC("StartGameRPC", RPCMode.All);
    }

    // Assign player-specific variables
    [RPC]
    void AssignVariables(int spawnPoint, float colorR, float colorG, float colorB, float colorA)
    {
        this.spawnPoint = spawnPoint;
        this.playerColor = new Color(colorR, colorG, colorB, colorA);
    }

    // Start game remote call
    [RPC]
    void StartGameRPC()
    {
        GameObject.Find("preview_state").SetActive(false);
        this.arenaSettings.cameraTransform.gameObject.SetActive(true);

        this.gameStarted = true;
        this.createDriverObject(this.spawnPoint, this.playerColor);
    }

    // Call when instantiated on network
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            StartCoroutine(this.StartGame());
    }
}


public static class ExtensionClass
{
    public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (System.Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}