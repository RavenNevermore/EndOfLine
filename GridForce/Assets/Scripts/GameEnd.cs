using UnityEngine;
using System;
using System.Collections;

public class GameEnd : MonoBehaviour {

	private bool engaged = false;
	private float delta_t = 0.0f;
    private bool matchEnded = false;

    public GUIText minutesText = null;
    public GUIText secondsText = null;
    public GameObject ingameUI = null;
    public GameObject resultsScreenUI = null;
    public GUIText player1Name = null;
    public GUIText player2Name = null;
    public GUIText player3Name = null;
    public GUIText player4Name = null;
    public GUIText player1Score = null;
    public GUIText player2Score = null;
    public GUIText player3Score = null;
    public GUIText player4Score = null;

    public float matchTimeInSeconds = 10;

    void Start()
    {
        this.minutesText.text = ((int)((float)(this.matchTimeInSeconds) / 60.0f)).ToString("0");
        this.secondsText.text = ((int)((float)(this.matchTimeInSeconds) % 60.0f)).ToString("00"); 
    }

    void Update()
    {
        if (this.matchEnded || !this.engaged || !this.TimeIsUp())
            return;

        if (Network.connections.Length <= 0 || this.networkView.isMine)
        {
            if (Network.connections.Length <= 0)
                this.EndMatchRPC();
            else
                this.networkView.RPC("EndMatchRPC", RPCMode.All);
        }

        this.EndMatchRPC();
    }

    public static void CloseConnections()
    {
        GameObject errorStateObject = GameObject.Find("ErrorState");
        if (errorStateObject != null)
            errorStateObject.GetComponent<ErrorState>().Hide();
        GameObject menuStateObject = GameObject.Find("MenuState");
        if (menuStateObject != null)
            menuStateObject.GetComponent<MenuState>().gameStarted = false;

        Network.Disconnect(200);
    }
	
    [RPC]
	public void EndMatchRPC()
    {
        GameEnd.CloseConnections();

        GameState activeGameState = this.GetComponent<GameState>();
        
        if (activeGameState != null)
        {
            activeGameState.EndGame();

            Array.Sort(activeGameState.players);
            Array.Reverse(activeGameState.players);

            for (int i = 0; i < activeGameState.players.GetLength(0); i++)
            {
                string name = activeGameState.players[i].name;

                if (name.Length > 11)
                    name = name.Substring(0, 10) + "...";

                switch (i)
                {
                    case 0:
                        this.player1Name.text = name;
                        this.player1Score.text = activeGameState.players[i].score.ToString();
                        this.player1Name.color = activeGameState.players[i].color;
                        this.player1Score.color = activeGameState.players[i].color;
                        break;

                    case 1:
                        this.player2Name.text = name;
                        this.player2Score.text = activeGameState.players[i].score.ToString();
                        this.player2Name.color = activeGameState.players[i].color;
                        this.player2Score.color = activeGameState.players[i].color;
                        break;

                    case 2:
                        this.player3Name.text = name;
                        this.player3Score.text = activeGameState.players[i].score.ToString();
                        this.player3Name.color = activeGameState.players[i].color;
                        this.player3Score.color = activeGameState.players[i].color;
                        break;

                    case 3:
                        this.player4Name.text = name;
                        this.player4Score.text = activeGameState.players[i].score.ToString();
                        this.player4Name.color = activeGameState.players[i].color;
                        this.player4Score.color = activeGameState.players[i].color;
                        break;
                }
            }
        }

        this.ingameUI.SetActive(false);
        this.resultsScreenUI.SetActive(true);
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
