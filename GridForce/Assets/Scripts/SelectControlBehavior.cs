using UnityEngine;
using System.Collections;

public class SelectControlBehavior : MonoBehaviour
{
    public MenuState menuState = null;
    public bool controlTypeValue = false;
    public GameObject selectControlButtons = null;
    public GameObject waitState = null;

    void OnMouseDown()
    {
        this.menuState.useButtonControls = this.controlTypeValue;
        this.menuState.ControlsSelected();
        this.waitState.SetActive(true);
        this.selectControlButtons.SetActive(false);
    }
}
