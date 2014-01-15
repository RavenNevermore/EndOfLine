using UnityEngine;
using System.Collections;

public class BtnJoinBehaviour : AbstractMenuBehaviour
{	
	public Camera menuCamera;
	public string hostName;
	public string hostIp;
	public int otherPlayers;

	void Start()
    {
		TextMesh text = this.GetComponentInChildren<TextMesh>();
		text.text = this.hostName + "(" + this.otherPlayers + ")";
	}
	
	
	void OnMouseDown()
    {
		this.gameState.type = MenuState.GameType.JOIN;
		
		this.gameState.hostName = this.hostName;
		
		this.gameState.hostIp = this.hostIp;
		
		this.switchToMenu("03_select_vehicle");
	}
}
