using UnityEngine;
using System.Collections;

public class GameEnd : MonoBehaviour {

	private bool engaged = false;
	private float delta_t = 0.0f;
	
	public float matchTimeInSeconds = 10;
	
	public static void EndGame(){
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            errorStateObject.GetComponent<ErrorState>().Hide();
        GameObject menuStateObject = GameObject.Find("MenuState");
        if (menuStateObject != null)
            menuStateObject.GetComponent<MenuState>().gameStarted = false;

        Network.Disconnect(200);
        Application.LoadLevel("MainMenu");
	}
	
	void Update () {
		if (!this.engaged || !this.TimeIsUp())
			return;
		
		GameEnd.EndGame();
	}
	
	private bool TimeIsUp(){
		this.delta_t += Time.deltaTime;
		if (this.delta_t >= this.matchTimeInSeconds){
			this.delta_t = 0.0f;
			return true;
		}
		return false;
	}
	
	public void Engage(){
		this.engaged = true;
	}
}
