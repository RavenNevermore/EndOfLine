using UnityEngine;
using System.Collections;

public class ExitButtonBehavior : MonoBehaviour
{
    void OnMouseDown()
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
}
