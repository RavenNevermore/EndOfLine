using UnityEngine;
using System.Collections;

public class BtnVehicleSelectBehaviour : AbstractMenuBehaviour {

	public int vehicleSelection = 0;

	void Start()
    {
		TextMesh text = this.GetComponentInChildren<TextMesh>();
		if (null != text)
        {
            text.text = "Vehicle " + (this.vehicleSelection + 1).ToString();
		}
	}

	void OnMouseDown()
    {
        this.gameState.vehicleSelection = this.vehicleSelection;
        Debug.Log("Selected Vehicle: " + this.vehicleSelection);
		this.gameState.startGame();
	}
}
