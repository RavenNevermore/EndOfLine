using UnityEngine;
using System;
using System.Collections;

public class MenuSelectSwitch : MonoBehaviour {

	public GameObject[] menuSelects;

	// Use this for initialization
	void Start () {
		this.deactivateAll();
		this.activateFirst();
	}

	void deactivateAll(){
		this.switchMenu(null, null);
	}

	void activateFirst(){
		if (this.menuSelects.Length > 0)
			this.activateGameObject(this.menuSelects[0]);
	}

	public void switchMenu(string nextMenu, string oldMenu){
		Debug.Log("Switching to "+nextMenu+" from "+oldMenu);
		foreach (GameObject select in this.menuSelects){
			if (null != nextMenu && select.name.Equals(nextMenu) 
			    		&& !select.activeSelf){
				this.activateGameObject(select);
				if (null != oldMenu)
					this.setReturnPathOnMenu(select, oldMenu);

			} else {
				select.SetActive(false);
			}
		}
	}

	void activateGameObject(GameObject obj){
            obj.SetActive(true);
            obj.SendMessage("OnMenuActivation");
	}

	void setReturnPathOnMenu(GameObject menu, string returnPath){
		BtnBackBehaviour backBtn = menu.transform.GetComponentInChildren<BtnBackBehaviour>();
		if (null != backBtn){
			Debug.Log("Found a back button. Setting return path: "+returnPath);
			backBtn.lastMenu = returnPath;
		}
	}
}
