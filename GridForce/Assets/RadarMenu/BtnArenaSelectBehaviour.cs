using UnityEngine;
using System.Collections;

public class BtnArenaSelectBehaviour : AbstractMenuBehaviour {

	public string arenaName;

	void Start(){
		TextMesh text = this.GetComponentInChildren<TextMesh>();
		if (null != text){
			text.text = this.arenaName;
		}
	}

	void OnMouseDown(){
		this.gameState.arenaName = this.arenaName;
		Debug.Log("Selected Arena: " + this.arenaName);

		this.gameState.color = "Red";

		this.switchToMenu("03_select_vehicle");
	}
}
