using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public ArenaSettings arenaSettings = null;    // Arena settings
    public Transform driverGameObject = null;     // Driver game object
    public Transform cameraTransform = null;      // Camera transform

    private List<Transform> activerPlayers = new List<Transform>();     // List of players

	// Use this for initialization
	void Start (){
		this.createPlayer();

		this.arenaSettings.bots -= 
			(this.arenaSettings.spawnPoints.Count + 1) - this.arenaSettings.bots;

		for (int i = 1; i <= this.arenaSettings.bots; i++){
			this.createDriver(i);
		}
	}

	void createPlayer(){
		DriverController driver = this.createDriver(0);
		driver.cameraTransform = this.cameraTransform;
	}

	DriverController createDriver(int spawnpoint){
		UnityEngine.Object newObject = UnityEngine.Object.Instantiate(
			this.driverGameObject, 
			this.arenaSettings.spawnPoints[spawnpoint].position, 
			this.arenaSettings.spawnPoints[spawnpoint].rotation);
		activerPlayers.Add((Transform)(newObject));
		
		DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
		driver.arenaSettings = this.arenaSettings;
		return driver;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (activerPlayers[0] == null)
        {
            UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.driverGameObject, this.arenaSettings.spawnPoints[0].position, this.arenaSettings.spawnPoints[0].rotation);
            DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
            driver.arenaSettings = this.arenaSettings;
            driver.cameraTransform = this.cameraTransform;
            activerPlayers[0] = ((Transform)(newObject));
        }	
	}
}
