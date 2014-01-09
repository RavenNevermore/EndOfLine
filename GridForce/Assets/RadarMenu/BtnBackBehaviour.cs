﻿using UnityEngine;
using System.Collections;

public class BtnBackBehaviour : AbstractMenuBehaviour {

	public string lastMenu;

	public void setLastMenu(string lastMenu){
		Debug.Log("setting lastMenu to "+lastMenu);
		this.lastMenu = lastMenu;
	}

	void OnMouseDown(){
		this.switchToMenu(this.lastMenu, false);
	}
}
