using UnityEngine;
using System.Collections;

public class ExitButtonBehavior : MonoBehaviour
{
    void OnMouseDown()
    {
        GameEnd.CloseConnections();
        Application.LoadLevel("MainMenu");
    }
}
