using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class GameState : MonoBehaviour
{
    public ArenaSettings arenaSettings = null;    // Arena settings
    public Transform driverPrefab = null;     // Driver game object
    private bool gameStarted = false;             // Has game started?
    public bool countdownOver = false;            // Is countdown done?

	public MenuState menuState;

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
    public PlayerData[] players = null;  // Players

    public int playerIndex = 0;    // This player's index
    public string playerName = "";    // This player's name
    private int spawnPoint = 0;    // This player's initial spawn point
    private Color playerColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);  // This player's color

    public int selectedMesh = 1;

    public GameObject boostItemGfx = null;
    public GameObject playerArrowPrefab = null;
    private PlayerArrowScript[] arrowGameObjects = null;


    // Use this for initialization
    void Start()
    {
		if (null == menuState)
            menuState = GameObject.Find("MenuState").GetComponent<MenuState>();

        this.playerName = menuState.playerName;
        this.selectedMesh = menuState.vehicleSelection;
        this.arenaSettings = GameObject.Find("Arena").GetComponent<ArenaSettings>();

        this.networkView.group = 0;

        for (int i = 0; i < 4; i++)
            this.spawnPointList.Add(i);
        for (int i = 0; i < colorArray.GetLength(0); i++)
            this.colorList.Add(colorArray[i]);

        colorList.Shuffle();
        spawnPointList.Shuffle();

        if (Network.connections.Length <= 0)
        {
            this.AssignVariables(1, 0, Mathf.Min(this.spawnPointList[0], this.arenaSettings.spawnPoints.Count - 1), colorList[0].r, colorList[0].g, colorList[0].b, colorList[0].a);
            this.AssignPlayerData(0, this.playerName, this.playerColor.r, this.playerColor.g, this.playerColor.b, this.playerColor.a);
            this.StartGameRPC();
        }
	}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; this.players != null && i < this.players.GetLength(0); i++)
        {
            this.players[i].Update();
        }

        if (!(this.countdownOver))
            this.GetComponent<DriverInput>().playerAction = PlayerAction.None;

        if (this.gameStarted && this.currentPlayerObject == null)
        {
            this.CreateDriverObject(this.FindFreeSpawnpoint(), this.playerColor);
        }

        if (this.gameStarted)
        {
            GameObject[] drivers = GameObject.FindGameObjectsWithTag("Driver");
            if (this.arrowGameObjects == null)
            {
                this.arrowGameObjects = new PlayerArrowScript[3];
                for (int i = 0; i < this.arrowGameObjects.GetLength(0); i++)
                    this.arrowGameObjects[i] = ((GameObject)(GameObject.Instantiate(this.playerArrowPrefab, this.currentPlayerObject.transform.position, Quaternion.identity))).GetComponent<PlayerArrowScript>();
            }

            for (int i = 0; i < this.arrowGameObjects.GetLength(0); i++)
                this.arrowGameObjects[i].transform.position = this.currentPlayerObject.position;

            int arrowIndex = 0;
            for (int i = 0; i < drivers.GetLength(0); i++)
            {
                if (drivers[i].transform != this.currentPlayerObject)
                {
                    this.arrowGameObjects[arrowIndex].arrowColor = drivers[i].GetComponent<DriverController>().mainColor;
                    this.arrowGameObjects[arrowIndex].target = drivers[i].transform;

                    arrowIndex++;
                }
            }
        }

        if (this.currentPlayerObject != null)
        {
            switch (this.currentPlayerObject.GetComponent<DriverController>().heldItem)
            {
                case ItemType.None:
                    this.boostItemGfx.SetActive(false);
                    break;

                case ItemType.Boost:
                    this.boostItemGfx.SetActive(true);
                    break;
            }
        }
    }


    // Instantiate a player object
	void CreateDriverObject(int spawnpoint, Color playerColor)
    {
        int i = Mathf.Min(spawnpoint, this.arenaSettings.spawnPoints.Count - 1);
        UnityEngine.Object newObject = null;
        if (Network.connections.Length > 0)
            newObject = UnityEngine.Network.Instantiate(this.driverPrefab, this.arenaSettings.spawnPoints[i].position, this.arenaSettings.spawnPoints[i].rotation, 0);
        else
            newObject = UnityEngine.Object.Instantiate(this.driverPrefab, this.arenaSettings.spawnPoints[i].position, this.arenaSettings.spawnPoints[i].rotation);
		DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
        driver.arenaSettings = this.arenaSettings;
        driver.cameraTransform = this.arenaSettings.cameraTransform;
        driver.mainColor = this.playerColor;
        driver.updateColor = true;
        driver.playerIndex = this.playerIndex;
        driver.playersRef = this.players;
        driver.SetMesh(this.selectedMesh);
        driver.gameStarted = this.countdownOver;
        DriverInput driverInput = this.GetComponent<DriverInput>();
        driverInput.playerAction = PlayerAction.None;
        driver.driverInput = driverInput;
        this.currentPlayerObject = ((Transform)(newObject));
    }

    public void CountdownOver()
    {
        this.countdownOver = true;
        DriverController driver = this.currentPlayerObject.GetComponent<DriverController>();
        driver.gameStarted = true;
    }

    // Try to find a free spawn point or else return a random spawn point
    int FindFreeSpawnpoint()
    {
        int spawnPoint = 0;

        spawnPointList.Shuffle();
        GameObject[] allDrivers = GameObject.FindGameObjectsWithTag("Driver");

        bool foundFreeSpawnPoint = false;
        for (int i = 0; i < this.spawnPointList.Count && !foundFreeSpawnPoint; i++)
        {
            int testSpawnPoint = this.spawnPointList[i];
            bool isOccupied = false;
            for (int j = 0; j < allDrivers.GetLength(0) && !isOccupied; j++)
            {
                if ((allDrivers[j].transform.position - this.arenaSettings.spawnPoints[testSpawnPoint].transform.position).magnitude < this.arenaSettings.minSpawnPointDistance)
                    isOccupied = true;
            }

            if (!isOccupied)
            {
                spawnPoint = testSpawnPoint;
                foundFreeSpawnPoint = true;
            }
        }

        if (foundFreeSpawnPoint)
            spawnPoint = Random.Range(0, this.arenaSettings.spawnPoints.Count - 1);

        return spawnPoint;
    }

    // Start hosted game
    public IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.1f);

        this.AssignVariables(Network.connections.Length + 1, 0, Mathf.Min(this.spawnPointList[0], this.arenaSettings.spawnPoints.Count - 1), colorList[0].r, colorList[0].g, colorList[0].b, colorList[0].a);

        int i = 0;        
        foreach (NetworkPlayer player in Network.connections)
        {
            i++;
            this.networkView.RPC("AssignVariables", player, Network.connections.Length + 1, i, Mathf.Min(this.spawnPointList[i], this.arenaSettings.spawnPoints.Count - 1), colorList[i].r, colorList[i].g, colorList[i].b, colorList[i].a);
        }

        yield return new WaitForSeconds(0.1f);

        this.networkView.RPC("RequestPlayerData", RPCMode.All);

        yield return new WaitForSeconds(0.1f);

        this.networkView.RPC("StartGameRPC", RPCMode.All);
    }

    // Assign player-specific variables
    [RPC]
    void AssignVariables(int numPlayers, int playerIndex, int spawnPoint, float colorR, float colorG, float colorB, float colorA)
    {
        this.players = new PlayerData[numPlayers];
        this.playerIndex = playerIndex;
        this.spawnPoint = spawnPoint;
        this.playerColor = new Color(colorR, colorG, colorB, colorA);
    }

    // Request player names
    [RPC]
    void RequestPlayerData()
    {
        this.networkView.RPC("AssignPlayerData", RPCMode.All, this.playerIndex, this.playerName, this.playerColor.r, this.playerColor.g, this.playerColor.b, this.playerColor.a);
    }

    // Assign player names
    [RPC]
    void AssignPlayerData(int playerIndex, string playerName, float colorR, float colorG, float colorB, float colorA)
    {
        this.players[playerIndex] = new PlayerData(playerIndex, playerName, new Color(colorR, colorG, colorB, colorA));
    }

    // Start game remote call
    [RPC]
    void StartGameRPC()
    {
        GameObject previewState = GameObject.Find("preview_state");
        if (previewState != null)
            previewState.SetActive(false);
        this.arenaSettings.cameraTransform.gameObject.SetActive(true);

        this.gameStarted = true;
        this.CreateDriverObject(this.spawnPoint, this.playerColor);
    }

    // Call when instantiated on network
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            StartCoroutine(this.StartGame());
    }



    // Contains information on a player
    public struct PlayerData
    {
        public int playerIndex;
        public string name;
        public int score;
        private GUIText playerNameText;
        private GUIText playerScoreText;

        public PlayerData(int playerIndex, string name, Color color)
        {
            this.playerIndex = playerIndex;
            this.name = name;
            this.score = 0;

            this.playerNameText = GameObject.Find("Player " + (this.playerIndex + 1).ToString()).GetComponent<GUIText>();
            this.playerScoreText = GameObject.Find("Player " + (this.playerIndex + 1).ToString() + " Score").GetComponent<GUIText>();
            this.playerNameText.color = color;
            this.playerScoreText.color = color;
            if (this.name.Length <= 11)
                this.playerNameText.text = this.name;
            else
                this.playerNameText.text = this.name.Substring(0, 10) + "...";
            this.playerScoreText.text = score.ToString();
        }

        public void Update()
        {
            if (this.playerScoreText != null)
            {
                this.playerScoreText.text = score.ToString();
            }
        }
    }

    // Synchronize data
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (this.players == null)
            return;

        int numPlayers = 0;
        if (stream.isWriting)
        {
            numPlayers = this.players.GetLength(0);
            stream.Serialize(ref numPlayers);
            for (int i = 0; i < numPlayers; i++)
                stream.Serialize(ref this.players[i].score);
        }
        else
        {
            stream.Serialize(ref numPlayers);
            for (int i = 0; i < numPlayers; i++)
                stream.Serialize(ref this.players[i].score);
        }
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