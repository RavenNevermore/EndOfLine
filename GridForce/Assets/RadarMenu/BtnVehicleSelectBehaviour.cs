using UnityEngine;
using System.Collections;

public class BtnVehicleSelectBehaviour : AbstractMenuBehaviour
{
	public int vehicleSelection = 0;

	void OnMouseDown()
    {
        this.gameState.vehicleSelection = this.vehicleSelection;
        Debug.Log("Selected Vehicle: " + this.vehicleSelection);
        this.gameState.StartGame();
	}
}
