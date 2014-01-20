using UnityEngine;
using System.Collections;

public class AbstractMenuBehaviour : MonoBehaviour {

	public void switchToMenu(string name){
		this.switchToMenu(name, true);
	}

	public void switchToMenu(string name, bool findReturnPath){
		string backPath = null;
		if (findReturnPath){
			backPath = this.transform.parent.gameObject.name;
		}

		GameObject root = GameObject.Find("root");
		MenuSelectSwitch menuSwitch = root.GetComponent<MenuSelectSwitch>();
		menuSwitch.switchMenu(name, backPath);
	}

	public MenuState gameState {
		get {
			GameObject state = GameObject.Find("MenuState");
			return state.GetComponent<MenuState>();
		}
	}
}
