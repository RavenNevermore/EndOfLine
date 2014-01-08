using UnityEngine;
using System.Collections;

public class BtnHostBehaviour : AbstractMenuBehaviour {

    public Camera menuCamera;


    void OnMouseDown(){
		this.gameState.type = MenuState.GameType.HOST;

		this.gameState.hostName = SystemInfo.deviceName;

		this.gameState.hostIp = Network.player.ipAddress;


		this.switchToMenu("02_select_arena");
	}
}
