using UnityEngine;
using System.Collections;

public class BtnJoinOtherBehaviour : AbstractMenuBehaviour {

	public int boxWidth;
	public int boxHeight;
	public int borderWidth;
	public Color guiBackground;

	Rect guiPosition;
	Rect textFieldPosition;
	Rect btnOkPosition;
	Rect btnEscPosition;
	bool enableGui;

	string ipToJoin;

	void Start(){
		this.guiPosition = new Rect(
			Screen.width / 2 - this.boxWidth / 2,
			Screen.height / 2 - this.boxHeight / 2,
			this.boxWidth,
			this.boxHeight);

		this.textFieldPosition = new Rect(
			this.guiPosition.x + this.borderWidth,
			this.guiPosition.y + this.guiPosition.height / 3,
			this.guiPosition.width - 2 * this.borderWidth,
			this.guiPosition.height / 3);

		this.btnOkPosition = new Rect(
			this.guiPosition.x + this.borderWidth,
			this.guiPosition.y + (this.guiPosition.height / 3) * 2,
			(this.guiPosition.width - 2 * this.borderWidth) / 2,
			this.guiPosition.height / 3);

		this.btnEscPosition = new Rect(
			this.guiPosition.x + this.borderWidth * 2 + this.btnOkPosition.width,
			this.guiPosition.y + (this.guiPosition.height / 3) * 2,
			(this.guiPosition.width - 2 * this.borderWidth) / 2,
			this.guiPosition.height / 3);

		this.enableGui = false;

		this.ipToJoin = "";
	}

	void OnMouseDown(){
		this.enableGui = true;
	}

	void OnGUI(){
		if (!this.enableGui)
			return;

		Event e = Event.current;
		
		if (e.keyCode == KeyCode.Return) {
			this.setIp();
		}

		GUI.backgroundColor = this.guiBackground;

		GUI.Box(this.guiPosition, "Enter the Ip of a Host.");

		this.ipToJoin = GUI.TextField(this.textFieldPosition, this.ipToJoin, 15);

		if (GUI.Button(this.btnOkPosition, "OK"))
			this.setIp();

		if (GUI.Button(this.btnEscPosition, "Cancel")){
			this.enableGui = false;
			this.ipToJoin = "";
		}
	}

	void setIp(){
		Debug.Log("Joining "+this.ipToJoin);

		this.gameState.type = MenuState.GameType.JOIN;
		
		this.gameState.hostName = "unknown";
		
		this.gameState.hostIp = this.ipToJoin;

		this.enableGui = false;
		this.ipToJoin = "";

		this.switchToMenu("03_select_vehicle");
	}
}
