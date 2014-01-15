using UnityEngine;
using System.Collections;

public class ErrorReturnToMainMenuButton : MonoBehaviour
{
    void OnMouseDown()
    {
        Application.LoadLevel("MainMenu");
    }
}
