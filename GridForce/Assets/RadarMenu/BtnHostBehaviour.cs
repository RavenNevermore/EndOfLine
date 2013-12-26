using UnityEngine;
using System.Collections;

public class BtnHostBehaviour : AbstractMenuBehaviour {

    public Camera menuCamera;


    void OnMouseDown(){
		Debug.Log("Yehaaa!" + this.name);
		this.gameState.type = MenuState.GameType.HOST;

		this.gameState.hostName = SystemInfo.deviceName;
		Debug.Log("My host name is " + this.gameState.hostName);

		this.gameState.hostIp = Network.player.ipAddress;
		Debug.Log("My host ip is " + this.gameState.hostIp);


		this.switchToMenu("02_select_arena");
	}
}
