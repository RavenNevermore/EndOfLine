using UnityEngine;
using System.Collections;

public class MenuState : MonoBehaviour {

	public enum GameType {HOST, JOIN};

	public GameType type;
	public string hostName;
	public string hostIp;

	public string arenaName;

	public string vehicleName;

	public string color;

	public void startGame(){
		Debug.Log("Starting game for " + this.arenaName);
		Application.LoadLevel(this.arenaName);
	}
}
