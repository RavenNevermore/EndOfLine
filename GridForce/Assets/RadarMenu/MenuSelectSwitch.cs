using UnityEngine;
using System.Collections;

public class MenuSelectSwitch : MonoBehaviour {

	public GameObject[] menuSelects;

	// Use this for initialization
	void Start () {
		this.deactivateAll();
		if (this.menuSelects.Length > 0)
			this.menuSelects[0].SetActive(true);
	}

	void deactivateAll(){
		this.switchMenu(null);
	}


	void switchMenu(string nextMenu){
		Debug.Log("Switching to "+nextMenu);
		foreach (GameObject select in this.menuSelects){
			if (null != nextMenu && select.name.Equals(nextMenu)){
				select.SetActive(true);
			} else {
				select.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
