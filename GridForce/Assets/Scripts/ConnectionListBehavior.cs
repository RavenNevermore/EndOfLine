using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConnectionListBehavior : AbstractMenuBehaviour
{
    public GameObject gameObjectPlayerOne = null;
    public GameObject gameObjectPlayerTwo = null;
    public GameObject gameObjectPlayerThree = null;
    public GameObject gameObjectPlayerFour = null;

    GUIText guiTextPlayerOne = null;
    GUIText guiTextPlayerTwo = null;
    GUIText guiTextPlayerThree = null;
    GUIText guiTextPlayerFour = null;

	// Use this for initialization
	void Start ()
    {
        this.guiTextPlayerOne = this.gameObjectPlayerOne.GetComponent<GUIText>();
        this.guiTextPlayerTwo = this.gameObjectPlayerTwo.GetComponent<GUIText>();
        this.guiTextPlayerThree = this.gameObjectPlayerThree.GetComponent<GUIText>();
        this.guiTextPlayerFour = this.gameObjectPlayerFour.GetComponent<GUIText>();

        if (Network.connections.Length > 0 && this.gameState.type.Equals(MenuState.GameType.JOIN))
            this.gameState.RequestNameUpdate(Network.player);
	}
	
	// Update is called once per frame
	void Update ()
    {
        this.guiTextPlayerOne.text = "";
        this.guiTextPlayerTwo.text = "";
        this.guiTextPlayerThree.text = "";
        this.guiTextPlayerFour.text = "";


        int currentPlayer = 1;

        if (this.gameState.type.Equals(MenuState.GameType.HOST))
        {
            this.gameState.playerNameList[0] = this.gameState.playerName;

            foreach (KeyValuePair<NetworkPlayer, string> valuePair in this.gameState.playerNames)
            {
                if (currentPlayer < this.gameState.playerNameList.GetLength(0))
                    this.gameState.playerNameList[currentPlayer] = valuePair.Value;
                currentPlayer++;
            }

            while (!(currentPlayer >= this.gameState.playerNameList.GetLength(0)))
            {
                this.gameState.playerNameList[currentPlayer] = "";
                currentPlayer++;
            }

            if (Network.connections.Length > 0 &&
                this.gameState.playerNameList[0] != this.gameState.playerNameListOld[0] || this.gameState.playerNameList[1] != this.gameState.playerNameListOld[1] ||
                this.gameState.playerNameList[2] != this.gameState.playerNameListOld[2] || this.gameState.playerNameList[3] != this.gameState.playerNameListOld[3])
            {
                this.gameState.UpdateNames(this.gameState.playerNameList[0], this.gameState.playerNameList[1], this.gameState.playerNameList[2], this.gameState.playerNameList[3]);
            }

            this.gameState.playerNameListOld[0] = this.gameState.playerNameList[0];
            this.gameState.playerNameListOld[1] = this.gameState.playerNameList[1];
            this.gameState.playerNameListOld[2] = this.gameState.playerNameList[2];
            this.gameState.playerNameListOld[3] = this.gameState.playerNameList[3];
        }


        currentPlayer = 0;
        foreach (string playerName in this.gameState.playerNameList)
        {
            string name = playerName;

            if (name.Length > 11)
                name = name.Substring(0, 10) + "...";

            switch (currentPlayer)
            {
                case 0:
                    this.guiTextPlayerOne.text = name;
                    break;
                case 1:
                    this.guiTextPlayerTwo.text = name;
                    break;
                case 2:
                    this.guiTextPlayerThree.text = name;
                    break;
                case 3:
                    this.guiTextPlayerFour.text = name;
                    break;
            }
            currentPlayer++;
        }
	}
}
