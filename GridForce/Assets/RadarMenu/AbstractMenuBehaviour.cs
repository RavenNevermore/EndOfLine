using UnityEngine;
using System.Collections;

public class AbstractMenuBehaviour : MonoBehaviour {

	public void switchToMenu(string name){
		GameObject root = GameObject.Find("root");
		root.BroadcastMessage("switchMenu", name);
	}

	public MenuState gameState {
		get {
			GameObject state = GameObject.Find("state");
			return state.GetComponent<MenuState>();
		}
	}
}
