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
	void Start ()
    {
        UnityEngine.Object newObject = UnityEngine.Object.Instantiate(this.driverGameObject, this.arenaSettings.spawnPoints[0].position, this.arenaSettings.spawnPoints[0].rotation);
        DriverController driver = ((Transform)(newObject)).gameObject.GetComponent<DriverController>();
        driver.arenaSettings = this.arenaSettings;
        driver.cameraTransform = this.cameraTransform;
        activerPlayers.Add((Transform)(newObject));
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
