using UnityEngine;
using System.Collections;

public class ButtonBehaviour : MonoBehaviour {

    public Camera menuCamera;

    public void OnSelected() {
        Debug.Log("Yehaaa!" + this.name);
        //this.menuCamera.transform.Translate(0, 0, 5);
        //Debug.Log("Schweinebacke...", this.menuCamera);
    }

    void OnMouseDown(){
        this.OnSelected();
    }
}
