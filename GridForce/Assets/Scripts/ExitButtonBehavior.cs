using UnityEngine;
using System.Collections;

public class ExitButtonBehavior : MonoBehaviour
{
    void OnMouseDown()
    {
        GameObject.Find("ErrorState").GetComponent<ErrorState>().Hide();
        GameObject.Find("MenuState").GetComponent<MenuState>().gameStarted = false;

        Network.Disconnect(200);
        Application.LoadLevel("MainMenu");
    }
}
