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
	}
	
	// Update is called once per frame
	void Update ()
    {
        this.guiTextPlayerOne.text = "";
        this.guiTextPlayerTwo.text = "";
        this.guiTextPlayerThree.text = "";
        this.guiTextPlayerFour.text = "";

        int currentPlayer = 0;
        foreach (KeyValuePair<NetworkPlayer, string> valuePair in this.gameState.playerNames)
        {
            string name = valuePair.Value;

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
