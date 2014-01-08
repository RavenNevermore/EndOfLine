using UnityEngine;
using System.Collections;

public class BtnJoinBehaviour : AbstractMenuBehaviour {
	
	public Camera menuCamera;
	public string hostName;
	public string hostIp;
	
	
	void OnMouseDown(){
		this.gameState.type = MenuState.GameType.JOIN;
		
		this.gameState.hostName = this.hostName;
		
		this.gameState.hostIp = this.hostIp;
		
		
		this.switchToMenu("03_select_vehicle");
	}
}
