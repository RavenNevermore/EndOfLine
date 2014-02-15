using UnityEngine;
using System.Collections;

public class OnScreenButtonBehavior : MonoBehaviour
{
    public bool buttonClicked = false;
    private bool firstFrame = true;

    void OnMouseDown()
    {
        this.firstFrame = true;
        this.buttonClicked = false;
    }

    void OnMouseDrag()
    {
        if (this.firstFrame)
        {
            this.buttonClicked = true;
            this.firstFrame = false;
        }
        else
        {
            this.buttonClicked = false;
            this.firstFrame = false;
        }
    }

    void OnMouseUp()
    {
        this.firstFrame = true;
        this.buttonClicked = false;
    }
}
