using UnityEngine;
using System;
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
    public static Dictionary<Color, string> colorNames = new Dictionary<Color, string>()
    {
        { colorArray[0], "RED" },
        { colorArray[1], "BLUE" },
        { colorArray[2], "PURPLE" },
        { colorArray[3], "GREEN" }
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
    public GameObject fakeItemBoxItemGfx = null;
    public GameObject sideBladesItemGfx = null;
    public GameObject immunityItemGfx = null;
    public GameObject invertControlsItemGfx = null;

    public GameObject ingameUI = null;
    public GameObject onScreenButtons = null;
    public bool useOnScreenButtons = false;

    public GameObject playerArrowPrefab = null;
    private PlayerArrowScript[] arrowGameObjects = null;

    public GameObject killFeedback = null;
    public float killFeedbackTimer = 0.0f;


    // Use this for initialization
    void Start()
    {
		if (null == menuState)
            menuState = GameObject.Find("MenuState").GetComponent<MenuState>();

        this.useOnScreenButtons = this.menuState.useButtonControls;

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

            string nameToUse = this.playerName;
            if (GameState.colorNames.ContainsKey(this.playerColor))
                nameToUse = GameState.colorNames[this.playerColor];

            this.AssignPlayerData(0, nameToUse, this.playerColor.r, this.playerColor.g, this.playerColor.b, this.playerColor.a);
            this.StartGameRPC();
        }
	}

    // Update is called once per frame
    void Update()
    {
        if (this.killFeedbackTimer > 0.0f)
        {
            this.killFeedbackTimer -= Time.deltaTime;
            if (this.killFeedbackTimer <= 0.0f)
                this.killFeedback.SetActive(false);
        }

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
			//Debug.Log("- "+drivers+" - "+drivers[0]+ " . "+drivers[1]+" --");
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
                if (arrowIndex < this.arrowGameObjects.GetLength(0) && drivers[i] != null && drivers[i].transform != this.currentPlayerObject && this.arrowGameObjects[arrowIndex] != null)
                {
                    DriverController driverController = drivers[i].GetComponent<DriverController>();
                    if (driverController != null)
                        this.arrowGameObjects[arrowIndex].arrowColor = driverController.mainColor;
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
                    this.sideBladesItemGfx.SetActive(false);
                    this.fakeItemBoxItemGfx.SetActive(false);
                    this.immunityItemGfx.SetActive(false);
                    this.invertControlsItemGfx.SetActive(false);
                    break;

                case ItemType.Boost:
                    this.boostItemGfx.SetActive(true);
                    this.sideBladesItemGfx.SetActive(false);
                    this.fakeItemBoxItemGfx.SetActive(false);
                    this.immunityItemGfx.SetActive(false);
                    this.invertControlsItemGfx.SetActive(false);
                    break;

                case ItemType.SideBlades:
                    this.boostItemGfx.SetActive(false);
                    this.sideBladesItemGfx.SetActive(true);
                    this.fakeItemBoxItemGfx.SetActive(false);
                    this.immunityItemGfx.SetActive(false);
                    this.invertControlsItemGfx.SetActive(false);
                    break;

                case ItemType.FakeItemBox:
                    this.boostItemGfx.SetActive(false);
                    this.sideBladesItemGfx.SetActive(false);
                    this.fakeItemBoxItemGfx.SetActive(true);
                    this.immunityItemGfx.SetActive(false);
                    this.invertControlsItemGfx.SetActive(false);
                    break;

                case ItemType.InvertControls:
                    this.boostItemGfx.SetActive(false);
                    this.sideBladesItemGfx.SetActive(false);
                    this.fakeItemBoxItemGfx.SetActive(false);
                    this.immunityItemGfx.SetActive(false);
                    this.invertControlsItemGfx.SetActive(true);
                    break;

                case ItemType.Immunity:
                    this.boostItemGfx.SetActive(false);
                    this.sideBladesItemGfx.SetActive(false);
                    this.fakeItemBoxItemGfx.SetActive(false);
                    this.immunityItemGfx.SetActive(true);
                    this.invertControlsItemGfx.SetActive(false);
                    break;
            }
        }
    }


    public void EndGame()
    {
        this.gameStarted = false;
        if (this.currentPlayerObject != null)
        {
            GameObject[] drivers = GameObject.FindGameObjectsWithTag("Driver");
            foreach (GameObject currentDriver in drivers)
            {
                DriverController driverController = currentDriver.GetComponent<DriverController>();
                if (driverController != null)
                {
                    driverController.gameStarted = false;
                    //UnityEngine.Object.Destroy(driverController.lineRenderer);
                    //driverController.Kill(-1, driverController.playerIndex);
                }
                //UnityEngine.Object.Destroy(currentDriver);
            }
            this.arenaSettings.cameraTransform.SendMessage("FollowTheLeader", this.arenaSettings.transform);
        }
    }


    // This function is automatically called when a player is killed
    // "killedPlayer" should always equal "this.playerIndex"
    // "killer" either contains the index of the killer or one of two values:
    // -1: Player was killed by non-drivable wall
    // -2: Player was killed by glitching out of level
    // Note that killer can equal killedPlayer if a player killed himself
    void ScoreFunction(int killer, int killedPlayer)
    {
        if (killer == -2)
            return;

        this.ResetMultiplier(killedPlayer);

        if (killer == killedPlayer || killer < 0)
            return;

        string playerName = "";
        Color playerColor = Color.white;
        if (killedPlayer >= 0 && killedPlayer < this.players.GetLength(0))
        {
            playerName = this.players[killedPlayer].name;
            playerColor = this.players[killedPlayer].color;
        }

        this.AddScore(killer, playerName, playerColor.r, playerColor.g, playerColor.b, playerColor.a);
    }

    // Reset multiplier on killed player
    public void ResetMultiplier(int playerIndex)
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("ResetMultiplierRPC", RPCMode.All, playerIndex);
        else
            this.ResetMultiplierRPC(playerIndex);
    }

    [RPC]
    public void ResetMultiplierRPC(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < this.players.GetLength(0))
            this.players[playerIndex].multiplier = 1;
    }

    // Add score to killer's score count
    public void AddScore(int playerIndex, string playerName, float colorR, float colorG, float colorB, float colorA)
    {
        if (Network.connections.Length > 0)
            this.networkView.RPC("AddScoreRPC", RPCMode.All, playerIndex, playerName, colorR, colorG, colorB, colorA);
        else
            this.AddScoreRPC(playerIndex, playerName, colorR, colorG, colorB, colorA);
    }

    [RPC]
    public void AddScoreRPC(int playerIndex, string playerName, float colorR, float colorG, float colorB, float colorA)
    {
        if (playerIndex >= 0 && playerIndex < this.players.GetLength(0))
        {
            int scoreAdd = (int)(this.arenaSettings.baseScore * this.players[playerIndex].multiplier);
            this.players[playerIndex].score += scoreAdd;
            this.players[playerIndex].multiplier += this.arenaSettings.multiplierIncrease;

            if (playerIndex == this.playerIndex)
            {
                GUIText feedbackText = this.killFeedback.GetComponent<GUIText>();
                if (feedbackText != null)
                {
                    string name = playerName;

                    if (name.Length > 11)
                        name = name.Substring(0, 10) + "...";

                    feedbackText.text = "YOU KILLED PLAYER " + name + "!\n SCORE +" + scoreAdd.ToString();
                    feedbackText.color = new Color(colorR, colorG, colorB, colorA);
                }

                this.killFeedbackTimer = 3.0f;
                this.killFeedback.SetActive(true);
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
        //driver.cameraTransform = this.arenaSettings.cameraTransform;
        driver.mainColor = this.playerColor;
        driver.updateColor = true;
        driver.playerIndex = this.playerIndex;
        driver.SetMesh(this.selectedMesh);
        driver.gameStarted = this.countdownOver;
        driver.ScoreFunction = ScoreFunction;
        DriverInput driverInput = this.GetComponent<DriverInput>();
        driverInput.playerAction = PlayerAction.None;
        driver.driverInput = driverInput;
        this.currentPlayerObject = ((Transform)(newObject));
		this.arenaSettings.cameraTransform.SendMessage("FollowTheLeader", this.currentPlayerObject);
    }

    public void CountdownOver()
    {
        this.countdownOver = true;
        DriverController driver = this.currentPlayerObject.GetComponent<DriverController>();
        driver.gameStarted = true;
		
		GameEnd end = this.GetComponent<GameEnd>();
		if (null != end){
			Debug.Log("This ends, when the song is over.");
			end.Engage();
		}
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
            spawnPoint = UnityEngine.Random.Range(0, this.arenaSettings.spawnPoints.Count - 1);

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
        string nameToUse = this.playerName;
        if (GameState.colorNames.ContainsKey(this.playerColor))
            nameToUse = GameState.colorNames[this.playerColor];

        this.networkView.RPC("AssignPlayerData", RPCMode.All, this.playerIndex, nameToUse, this.playerColor.r, this.playerColor.g, this.playerColor.b, this.playerColor.a);
    }

    // Assign player names
    [RPC]
    void AssignPlayerData(int playerIndex, string playerName, float colorR, float colorG, float colorB, float colorA)
    {
        this.ingameUI.SetActive(true);
        this.players[playerIndex] = new PlayerData( 
				playerIndex, 
				playerName, 
				new Color(	colorR, 
							colorG, 
							colorB, 
							colorA));
        if (!(this.gameStarted))
            this.ingameUI.SetActive(false);
    }

    // Start game remote call
    [RPC]
    void StartGameRPC()
    {
        GameObject previewState = GameObject.Find("preview_state");
        if (previewState != null)
            previewState.SetActive(false);
        this.arenaSettings.cameraTransform.gameObject.SetActive(true);

        this.ingameUI.SetActive(true);
        if (this.useOnScreenButtons)
            this.onScreenButtons.SetActive(true);

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
    public struct PlayerData : IComparable
    {
        public int playerIndex;
        public string name;
        public int score;
		public float multiplier;
        public Color color;
		
        private GUIText playerNameText;
        private GUIText playerScoreText;

        public PlayerData(int playerIndex, 
						  string name, 
						  Color color)
        {
            this.playerIndex = playerIndex;
            this.name = name;
            this.score = 0;
			this.multiplier = 1.0f;
            this.color = color;

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

        public int CompareTo(object obj)
        {
            if (obj is PlayerData)
            {
                return this.score.CompareTo(((PlayerData)(obj)).score);
            }

            throw new ArgumentException("Object is not a PlayerData");
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
            {
                stream.Serialize(ref this.players[i].score);
                stream.Serialize(ref this.players[i].multiplier);
            }
        }
        else
        {
            stream.Serialize(ref numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                stream.Serialize(ref this.players[i].score);
                stream.Serialize(ref this.players[i].multiplier);
            }
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