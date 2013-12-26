using UnityEngine;
using System.Collections;

public class AbstractMenuBehaviour : MonoBehaviour {

	public void switchToMenu(string name){
		GameObject root = GameObject.Find("root");
		root.BroadcastMessage("switchMenu", name);
	}

	public MenuState gameState {
		get {
			GameObject root = GameObject.Find("root");
			return root.GetComponent<MenuState>();
		}
	}
}
