using UnityEngine;
using System.Collections;

public class GameEnd : MonoBehaviour {

	private bool engaged = false;
	private float delta_t = 0.0f;
    private bool matchEnded = false;

    public GUIText minutesText = null;
    public GUIText secondsText = null;

    public float matchTimeInSeconds = 10;

    void Start()
    {
        this.minutesText.text = ((int)((float)(this.matchTimeInSeconds) / 60.0f)).ToString("0");
        this.secondsText.text = ((int)((float)(this.matchTimeInSeconds) % 60.0f)).ToString("0"); 
    }

    void Update()
    {
        if (!this.engaged || !this.TimeIsUp())
            return;

        GameEnd.EndGame();
    }
	
	public static void EndGame()
    {
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            errorStateObject.GetComponent<ErrorState>().Hide();
        GameObject menuStateObject = GameObject.Find("MenuState");
        if (menuStateObject != null)
            menuStateObject.GetComponent<MenuState>().gameStarted = false;

        Network.Disconnect(200);
        Application.LoadLevel("MainMenu");
	}
	
	private bool TimeIsUp()
    {
		this.delta_t += Time.deltaTime;

        if (!(this.matchEnded))
        {
            float timeLeft = (float)(this.matchTimeInSeconds) - this.delta_t + 1.0f;
            int minutes = (int)(timeLeft / 60.0f);
            int seconds = (int)(timeLeft % 60.0f);
            this.minutesText.text = minutes.ToString("0");
            this.secondsText.text = seconds.ToString("00");
        }
        else
        {
            this.minutesText.text = "0";
            this.secondsText.text = "00";
        }


		if (this.delta_t >= this.matchTimeInSeconds)
        {
			this.delta_t = 0.0f;
            this.matchEnded = true;
			return true;
		}
		return false;
	}
	
	public void Engage()
    {
		this.engaged = true;
	}
}
