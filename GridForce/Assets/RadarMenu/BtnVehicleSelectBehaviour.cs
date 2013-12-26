using UnityEngine;
using System.Collections;

public class BtnVehicleSelectBehaviour : AbstractMenuBehaviour {

	public string vehicleName;

	void Start(){
		TextMesh text = this.GetComponentInChildren<TextMesh>();
		if (null != text){
			text.text = this.vehicleName;
		}
	}

	void OnMouseDown(){
		this.gameState.vehicleName = this.vehicleName;
		Debug.Log("Selected Vehicle: " + this.vehicleName);
		this.gameState.startGame();
	}
}
